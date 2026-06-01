using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Youdemy.Data;
using Youdemy.Models;

namespace Youdemy.Controllers
{
    public class LessonsController : Controller
    {
        private readonly YoudemyDbContext _context;

        public LessonsController(YoudemyDbContext context)
        {
            _context = context;
        }

        // GET: /Lessons/Create?courseId=5
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Create(int courseId)
        {
            var teacher = await _context.Users.FindAsync(GetUserId());
            if (teacher == null || !teacher.IsApproved)
            {
                return RedirectToAction("Manage", "Courses");
            }

            var course = await _context.Courses.FindAsync(courseId);
            if (course == null || course.TeacherId != GetUserId())
            {
                return Unauthorized();
            }

            var nextOrder = await _context.Lessons
                .Where(l => l.CourseId == courseId)
                .Select(l => (int?)l.Order)
                .MaxAsync() ?? 0;

            var model = new LessonCreateEditViewModel
            {
                CourseId = courseId,
                CourseTitle = course.Title,
                Order = nextOrder + 1
            };

            return View(model);
        }

        // POST: /Lessons/Create
        [HttpPost]
        [Authorize(Roles = "Teacher")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LessonCreateEditViewModel model)
        {
            var teacher = await _context.Users.FindAsync(GetUserId());
            if (teacher == null || !teacher.IsApproved)
            {
                return RedirectToAction("Manage", "Courses");
            }

            var course = await _context.Courses.FindAsync(model.CourseId);
            if (course == null || course.TeacherId != GetUserId())
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                model.CourseTitle = course.Title;
                return View(model);
            }

            var lesson = new Lesson
            {
                Title = model.Title,
                Description = model.Description ?? string.Empty,
                VideoUrl = NormalizeVideoUrl(model.VideoUrl),
                Order = model.Order,
                CourseId = model.CourseId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Lessons.Add(lesson);
            await _context.SaveChangesAsync();

            // After adding a new lesson, pre-create progress entries for any students already enrolled in this course
            var studentIds = await _context.Enrollments
                .Where(e => e.CourseId == model.CourseId)
                .Select(e => e.StudentId)
                .ToListAsync();

            foreach (var studentId in studentIds)
            {
                _context.LessonProgresses.Add(new LessonProgress
                {
                    StudentId = studentId,
                    LessonId = lesson.Id,
                    IsCompleted = false,
                    CompletedAt = DateTime.UtcNow
                });
            }
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Courses", new { id = model.CourseId });
        }

        // GET: /Lessons/Edit/5
        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> Edit(int id)
        {
            var teacher = await _context.Users.FindAsync(GetUserId());
            if (teacher == null || !teacher.IsApproved)
            {
                return RedirectToAction("Manage", "Courses");
            }

            var lesson = await _context.Lessons
                .Include(l => l.Course)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (lesson == null || lesson.Course?.TeacherId != GetUserId())
            {
                return Unauthorized();
            }

            var model = new LessonCreateEditViewModel
            {
                Id = lesson.Id,
                CourseId = lesson.CourseId,
                CourseTitle = lesson.Course!.Title,
                Title = lesson.Title,
                Description = lesson.Description,
                VideoUrl = lesson.VideoUrl,
                Order = lesson.Order
            };

            return View(model);
        }

        // POST: /Lessons/Edit/5
        [HttpPost]
        [Authorize(Roles = "Teacher")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, LessonCreateEditViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            var teacher = await _context.Users.FindAsync(GetUserId());
            if (teacher == null || !teacher.IsApproved)
            {
                return RedirectToAction("Manage", "Courses");
            }

            var lesson = await _context.Lessons
                .Include(l => l.Course)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (lesson == null || lesson.Course?.TeacherId != GetUserId())
            {
                return Unauthorized();
            }

            if (!ModelState.IsValid)
            {
                model.CourseTitle = lesson.Course!.Title;
                return View(model);
            }

            lesson.Title = model.Title;
            lesson.Description = model.Description ?? string.Empty;
            lesson.VideoUrl = NormalizeVideoUrl(model.VideoUrl);
            lesson.Order = model.Order;

            _context.Entry(lesson).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Courses", new { id = lesson.CourseId });
        }

        // POST: /Lessons/Delete/5
        [HttpPost]
        [Authorize(Roles = "Teacher")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var teacher = await _context.Users.FindAsync(GetUserId());
            if (teacher == null || !teacher.IsApproved)
            {
                return RedirectToAction("Manage", "Courses");
            }

            var lesson = await _context.Lessons
                .Include(l => l.Course)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (lesson == null || lesson.Course?.TeacherId != GetUserId())
            {
                return Unauthorized();
            }

            var courseId = lesson.CourseId;
            _context.Lessons.Remove(lesson);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", "Courses", new { id = courseId });
        }

        // POST: /Lessons/ToggleComplete
        [HttpPost]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> ToggleComplete(int lessonId, bool isCompleted)
        {
            var studentId = GetUserId();

            var progress = await _context.LessonProgresses
                .Include(lp => lp.Lesson)
                .FirstOrDefaultAsync(lp => lp.StudentId == studentId && lp.LessonId == lessonId);

            if (progress == null)
            {
                var lesson = await _context.Lessons.FindAsync(lessonId);
                if (lesson == null)
                {
                    return NotFound(new { success = false, message = "Ders bulunamadı." });
                }

                // Verify enrollment
                var isEnrolled = await _context.Enrollments
                    .AnyAsync(e => e.StudentId == studentId && e.CourseId == lesson.CourseId);

                if (!isEnrolled)
                {
                    return Unauthorized(new { success = false, message = "Bu kursa kayıtlı değilsiniz." });
                }

                progress = new LessonProgress
                {
                    StudentId = studentId,
                    LessonId = lessonId,
                    IsCompleted = isCompleted,
                    CompletedAt = DateTime.UtcNow
                };

                _context.LessonProgresses.Add(progress);
                await _context.SaveChangesAsync();

                // Refetch to populate Navigation properties
                progress = await _context.LessonProgresses
                    .Include(lp => lp.Lesson)
                    .FirstAsync(lp => lp.Id == progress.Id);
            }
            else
            {
                progress.IsCompleted = isCompleted;
                progress.CompletedAt = DateTime.UtcNow;
                _context.Entry(progress).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }

            var courseId = progress.Lesson!.CourseId;
            var totalCount = await _context.Lessons.CountAsync(l => l.CourseId == courseId);
            var completedCount = await _context.LessonProgresses
                .CountAsync(lp => lp.StudentId == studentId && lp.Lesson.CourseId == courseId && lp.IsCompleted);

            double percentage = totalCount > 0 ? (double)completedCount / totalCount * 100 : 0.0;

            return Json(new
            {
                success = true,
                completedCount = completedCount,
                totalCount = totalCount,
                progressPercentage = Math.Round(percentage, 1)
            });
        }

        private int GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null ? int.Parse(claim.Value) : 0;
        }

        private string NormalizeVideoUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return string.Empty;
            }

            // Already an embed link
            if (url.Contains("/embed/"))
            {
                return url;
            }

            // Standard URL: youtube.com/watch?v=VIDEO_ID
            if (url.Contains("youtube.com/watch"))
            {
                try
                {
                    var uri = new Uri(url);
                    var query = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);
                    if (query.TryGetValue("v", out var videoId))
                    {
                        return $"https://www.youtube.com/embed/{videoId.FirstOrDefault()}";
                    }
                }
                catch { }
            }

            // Short URL: youtu.be/VIDEO_ID
            if (url.Contains("youtu.be/"))
            {
                try
                {
                    var uri = new Uri(url);
                    var videoId = uri.AbsolutePath.TrimStart('/');
                    return $"https://www.youtube.com/embed/{videoId}";
                }
                catch { }
            }

            return url;
        }
    }
}
