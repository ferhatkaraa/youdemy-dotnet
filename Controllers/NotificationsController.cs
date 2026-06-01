using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Youdemy.Data;
using Youdemy.Models;

namespace Youdemy.Controllers
{
    [Authorize]
    public class NotificationsController : Controller
    {
        private readonly YoudemyDbContext _context;

        public NotificationsController(YoudemyDbContext context)
        {
            _context = context;
        }

        [Authorize]
        public async Task<IActionResult> Inbox()
        {
            var userId = GetUserId();
            var notifications = await _context.Notifications
                .Include(n => n.Sender)
                .Where(n => n.RecipientId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            var unreadNotifications = notifications.Where(n => !n.IsRead).ToList();
            if (unreadNotifications.Any())
            {
                unreadNotifications.ForEach(n => n.IsRead = true);
                await _context.SaveChangesAsync();
            }

            return View(notifications);
        }

        [Authorize(Roles = "Teacher")]
        public async Task<IActionResult> SendToStudents(int? courseId = null)
        {
            var teacherId = GetUserId();
            var teacher = await _context.Users.FindAsync(teacherId);
            if (teacher == null || !teacher.IsApproved)
            {
                return RedirectToAction("Manage", "Courses");
            }

            var studentIdsQuery = _context.Enrollments
                .Where(e => e.Course != null && e.Course.TeacherId == teacherId);

            if (courseId.HasValue)
            {
                studentIdsQuery = studentIdsQuery.Where(e => e.CourseId == courseId.Value);
            }

            var studentIds = await studentIdsQuery
                .Select(e => e.StudentId)
                .Distinct()
                .ToListAsync();

            var students = await _context.Users
                .Where(u => studentIds.Contains(u.Id))
                .OrderBy(u => u.Username)
                .ToListAsync();

            var model = new TeacherNotificationViewModel
            {
                AvailableStudents = students,
                SelectedCourseId = courseId
            };

            if (courseId.HasValue)
            {
                model.CourseTitle = await _context.Courses
                    .Where(c => c.Id == courseId.Value && c.TeacherId == teacherId)
                    .Select(c => c.Title)
                    .FirstOrDefaultAsync();
            }

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Teacher")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendToStudents(TeacherNotificationViewModel model)
        {
            var teacherId = GetUserId();
            var teacher = await _context.Users.FindAsync(teacherId);
            if (teacher == null || !teacher.IsApproved)
            {
                return RedirectToAction("Manage", "Courses");
            }

            var studentIdsQuery = _context.Enrollments
                .Where(e => e.Course != null && e.Course.TeacherId == teacherId);

            if (model.SelectedCourseId.HasValue)
            {
                studentIdsQuery = studentIdsQuery.Where(e => e.CourseId == model.SelectedCourseId.Value);
            }

            var studentIds = await studentIdsQuery
                .Select(e => e.StudentId)
                .Distinct()
                .ToListAsync();

            var students = await _context.Users
                .Where(u => studentIds.Contains(u.Id))
                .OrderBy(u => u.Username)
                .ToListAsync();

            model.AvailableStudents = students;
            if (model.SelectedCourseId.HasValue)
            {
                model.CourseTitle = await _context.Courses
                    .Where(c => c.Id == model.SelectedCourseId.Value && c.TeacherId == teacherId)
                    .Select(c => c.Title)
                    .FirstOrDefaultAsync();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var recipientsQuery = _context.Users.Where(u => studentIds.Contains(u.Id));
            if (model.SelectedStudentIds != null && model.SelectedStudentIds.Length > 0)
            {
                recipientsQuery = recipientsQuery.Where(u => model.SelectedStudentIds.Contains(u.Id));
            }

            var recipients = await recipientsQuery.ToListAsync();
            foreach (var recipient in recipients)
            {
                _context.Notifications.Add(new Notification
                {
                    RecipientId = recipient.Id,
                    SenderId = teacherId,
                    Message = model.Message,
                    CreatedAt = System.DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"{recipients.Count} öğrenciye bildirim gönderildi.";
            return RedirectToAction("SendToStudents", new { courseId = model.SelectedCourseId });
        }

        private int GetUserId()
        {
            var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            return claim != null ? int.Parse(claim.Value) : 0;
        }
    }
}
