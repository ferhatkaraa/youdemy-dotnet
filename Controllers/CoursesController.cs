using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Youdemy.Data;
using Youdemy.Filters;
using Youdemy.Models;

namespace Youdemy.Controllers
{
    public class CoursesController : Controller
    {
        private readonly YoudemyDbContext _context;

        public CoursesController(YoudemyDbContext context)
        {
            _context = context;
        }

        // GET: /Courses/Details/5
        public async Task<IActionResult> Details(int id, string? error = null)
        {
            var course = await _context.Courses
                .Include(c => c.Teacher)
                .Include(c => c.Lessons.OrderBy(l => l.Order).ThenBy(l => l.Id))
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null)
            {
                return NotFound();
            }

            bool isEnrolled = false;
            bool isTeacherOfCourse = false;

            if (User.Identity?.IsAuthenticated ?? false)
            {
                var userId = GetUserId();
                var role = User.FindFirst(ClaimTypes.Role)?.Value;

                if (role == UserRole.Student.ToString())
                {
                    isEnrolled = await _context.Enrollments
                        .AnyAsync(e => e.StudentId == userId && e.CourseId == id);
                }
                else if (role == UserRole.Teacher.ToString())
                {
                    isTeacherOfCourse = course.TeacherId == userId;
                }
            }

            ViewData["IsEnrolled"] = isEnrolled;
            ViewData["IsTeacherOfCourse"] = isTeacherOfCourse;
            ViewData["ErrorMessage"] = error;

            return View(course);
        }

        [Authorize(Roles = "Student")]
        public async Task<IActionResult> MyCourses()
        {
            var studentId = GetUserId();
            var enrollments = await _context.Enrollments
                .Where(e => e.StudentId == studentId)
                .Include(e => e.Course)
                    .ThenInclude(c => c.Teacher)
                .Include(e => e.Course)
                    .ThenInclude(c => c.Lessons)
                .OrderByDescending(e => e.EnrolledAt)
                .ToListAsync();

            var courseIds = enrollments.Select(e => e.CourseId).ToList();
            var progressCounts = await _context.LessonProgresses
                .Where(lp => lp.StudentId == studentId && courseIds.Contains(lp.Lesson.CourseId) && lp.IsCompleted)
                .GroupBy(lp => lp.Lesson.CourseId)
                .Select(g => new { CourseId = g.Key, Count = g.Count() })
                .ToListAsync();

            var model = enrollments.Select(e =>
            {
                var course = e.Course!;
                var totalLessons = course.Lessons.Count;
                var completedLessons = progressCounts.FirstOrDefault(p => p.CourseId == course.Id)?.Count ?? 0;
                var percent = totalLessons > 0 ? Math.Round((double)completedLessons / totalLessons * 100, 1) : 0.0;

                return new StudentCourseProgressViewModel
                {
                    Course = course,
                    EnrolledAt = e.EnrolledAt,
                    TotalLessonsCount = totalLessons,
                    CompletedLessonsCount = completedLessons,
                    ProgressPercentage = percent
                };
            }).ToList();

            return View(model);
        }

        // POST: /Courses/Enroll/5
        [HttpPost]
        [Authorize(Roles = "Student")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enroll(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course == null)
            {
                return NotFound();
            }

            var studentId = GetUserId();

            // Check if already enrolled
            var alreadyEnrolled = await _context.Enrollments
                .AnyAsync(e => e.StudentId == studentId && e.CourseId == id);

            if (!alreadyEnrolled)
            {
                // Create enrollment
                var enrollment = new Enrollment
                {
                    StudentId = studentId,
                    CourseId = id,
                    EnrolledAt = DateTime.UtcNow
                };

                _context.Enrollments.Add(enrollment);
                await _context.SaveChangesAsync();

                // Pre-generate LessonProgress records for all lessons in this course
                var lessons = await _context.Lessons.Where(l => l.CourseId == id).ToListAsync();
                foreach (var lesson in lessons)
                {
                    var progressExists = await _context.LessonProgresses
                        .AnyAsync(lp => lp.StudentId == studentId && lp.LessonId == lesson.Id);

                    if (!progressExists)
                    {
                        _context.LessonProgresses.Add(new LessonProgress
                        {
                            StudentId = studentId,
                            LessonId = lesson.Id,
                            IsCompleted = false,
                            CompletedAt = DateTime.UtcNow
                        });
                    }
                }
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Watch", new { id = id });
        }

        // GET: /Courses/Watch/5
        [CourseAuthorize]
        public async Task<IActionResult> Watch(int id, int? lessonId = null)
        {
            var course = await _context.Courses
                .Include(c => c.Teacher)
                .Include(c => c.Lessons.OrderBy(l => l.Order).ThenBy(l => l.Id))
                .FirstOrDefaultAsync(c => c.Id == id);

            if (course == null)
            {
                return NotFound();
            }

            if (!course.Lessons.Any())
            {
                // Return watch view with a message that there are no lessons yet
                return View("NoLessons", course);
            }

            Lesson? activeLesson = null;
            if (lessonId.HasValue)
            {
                activeLesson = course.Lessons.FirstOrDefault(l => l.Id == lessonId.Value);
            }

            // Fallback to first lesson
            if (activeLesson == null)
            {
                activeLesson = course.Lessons.First();
            }

            // Calculate progress stats for Students
            int completedCount = 0;
            int totalCount = course.Lessons.Count;
            double percentage = 0.0;

            if (User.FindFirst(ClaimTypes.Role)?.Value == UserRole.Student.ToString())
            {
                var studentId = GetUserId();
                completedCount = await _context.LessonProgresses
                    .CountAsync(lp => lp.StudentId == studentId && lp.Lesson.CourseId == id && lp.IsCompleted);

                if (totalCount > 0)
                {
                    percentage = (double)completedCount / totalCount * 100;
                }
            }

            var viewModel = new CourseWatchViewModel
            {
                Course = course,
                ActiveLesson = activeLesson,
                CompletedLessonsCount = completedCount,
                TotalLessonsCount = totalCount,
                ProgressPercentage = Math.Round(percentage, 1)
            };

            return View(viewModel);
        }

        // GET: /Courses/Manage
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Manage()
        {
            var teacherId = GetUserId();
            var teacher = await _context.Users.FindAsync(teacherId);
            if (teacher == null)
            {
                return Unauthorized();
            }

            if (!teacher.IsApproved)
            {
                ViewData["ApprovalMessage"] = "Öğretmen hesabınız yönetici onayı bekliyor. Kurs ekleme ve ders ekleme özellikleri onaylandıktan sonra açılacaktır.";
            }

            var courses = await _context.Courses
                .Where(c => c.TeacherId == teacherId)
                .Include(c => c.Lessons)
                .Include(c => c.Enrollments)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return View(courses);
        }

        // GET: /Courses/Create
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Create()
        {
            var teacher = await _context.Users.FindAsync(GetUserId());
            if (teacher == null || !teacher.IsApproved)
            {
                return RedirectToAction("Manage");
            }

            return View(new CourseCreateEditViewModel());
        }

        // POST: /Courses/Create
        [HttpPost]
        [Authorize(Roles = "Teacher")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CourseCreateEditViewModel model)
        {
            var teacher = await _context.Users.FindAsync(GetUserId());
            if (teacher == null || !teacher.IsApproved)
            {
                return RedirectToAction("Manage");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var teacherId = GetUserId();
            var course = new Course
            {
                Title = model.Title,
                Description = model.Description,
                ImageUrl = string.IsNullOrWhiteSpace(model.ImageUrl) ? "/images/course-default.png" : model.ImageUrl,
                TeacherId = teacherId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            return RedirectToAction("Manage");
        }

        // GET: /Courses/Edit/5
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Edit(int id)
        {
            var teacher = await _context.Users.FindAsync(GetUserId());
            if (teacher == null || !teacher.IsApproved)
            {
                return RedirectToAction("Manage");
            }

            var course = await _context.Courses.FindAsync(id);
            if (course == null || course.TeacherId != GetUserId())
            {
                return Unauthorized();
            }

            var model = new CourseCreateEditViewModel
            {
                Id = course.Id,
                Title = course.Title,
                Description = course.Description,
                ImageUrl = course.ImageUrl
            };

            return View(model);
        }

        // POST: /Courses/Edit/5
        [HttpPost]
        [Authorize(Roles = "Teacher")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CourseCreateEditViewModel model)
        {
            var teacher = await _context.Users.FindAsync(GetUserId());
            if (teacher == null || !teacher.IsApproved)
            {
                return RedirectToAction("Manage");
            }

            if (id != model.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var course = await _context.Courses.FindAsync(id);
            if (course == null || course.TeacherId != GetUserId())
            {
                return Unauthorized();
            }

            course.Title = model.Title;
            course.Description = model.Description;
            course.ImageUrl = string.IsNullOrWhiteSpace(model.ImageUrl) ? "/images/course-default.png" : model.ImageUrl;

            _context.Entry(course).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return RedirectToAction("Manage");
        }

        // POST: /Courses/Delete/5
        [HttpPost]
        [Authorize(Roles = "Teacher")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var teacher = await _context.Users.FindAsync(GetUserId());
            if (teacher == null || !teacher.IsApproved)
            {
                return RedirectToAction("Manage");
            }

            var course = await _context.Courses.FindAsync(id);
            if (course == null || course.TeacherId != GetUserId())
            {
                return Unauthorized();
            }

            _context.Courses.Remove(course);
            await _context.SaveChangesAsync();

            return RedirectToAction("Manage");
        }

        private int GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null ? int.Parse(claim.Value) : 0;
        }
    }
}
