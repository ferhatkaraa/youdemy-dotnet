using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Youdemy.Data;
using Youdemy.Models;

namespace Youdemy.ViewComponents
{
    public class CourseSyllabusViewComponent : ViewComponent
    {
        private readonly YoudemyDbContext _context;

        public CourseSyllabusViewComponent(YoudemyDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync(int courseId, int activeLessonId)
        {
            var lessons = await _context.Lessons
                .Where(l => l.CourseId == courseId)
                .OrderBy(l => l.Order)
                .ThenBy(l => l.Id)
                .ToListAsync();

            var user = HttpContext.User;
            var isStudent = user.FindFirst(ClaimTypes.Role)?.Value == UserRole.Student.ToString();
            var completedLessonIds = new HashSet<int>();

            if (isStudent && (user.Identity?.IsAuthenticated ?? false))
            {
                var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out var studentId))
                {
                    var completedList = await _context.LessonProgresses
                        .Where(lp => lp.StudentId == studentId && lp.Lesson.CourseId == courseId && lp.IsCompleted)
                        .Select(lp => lp.LessonId)
                        .ToListAsync();

                    completedLessonIds = new HashSet<int>(completedList);
                }
            }

            ViewData["ActiveLessonId"] = activeLessonId;
            ViewData["CompletedLessonIds"] = completedLessonIds;
            ViewData["IsStudent"] = isStudent;

            return View(lessons);
        }
    }
}
