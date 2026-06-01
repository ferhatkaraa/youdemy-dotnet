using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Youdemy.Data;
using Youdemy.Models;

namespace Youdemy.Filters
{
    public class CourseAuthorizeAttribute : TypeFilterAttribute
    {
        public CourseAuthorizeAttribute() : base(typeof(CourseAuthorizeFilter))
        {
        }
    }

    public class CourseAuthorizeFilter : IAsyncAuthorizationFilter
    {
        private readonly YoudemyDbContext _dbContext;

        public CourseAuthorizeFilter(YoudemyDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var HttpContext = context.HttpContext;
            var user = HttpContext.User;

            // If not logged in, redirect to login page
            if (!user.Identity?.IsAuthenticated ?? true)
            {
                context.Result = new RedirectToActionResult("Login", "Auth", null);
                return;
            }

            // Extract claims
            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
            var roleClaim = user.FindFirst(ClaimTypes.Role);

            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            {
                context.Result = new RedirectToActionResult("Login", "Auth", null);
                return;
            }

            // Teachers are allowed to view any course (e.g. for audit or own course management)
            if (roleClaim != null && roleClaim.Value == UserRole.Teacher.ToString())
            {
                return;
            }

            // Students must be enrolled in the course. Let's find courseId in Route, Query, or Form.
            int courseId = 0;
            if (context.RouteData.Values.TryGetValue("courseId", out var routeCourseId) && routeCourseId != null)
            {
                int.TryParse(routeCourseId.ToString(), out courseId);
            }
            
            if (courseId == 0 && context.RouteData.Values.TryGetValue("id", out var routeId) && routeId != null)
            {
                int.TryParse(routeId.ToString(), out courseId);
            }

            if (courseId == 0 && HttpContext.Request.Query.TryGetValue("courseId", out var queryCourseId))
            {
                int.TryParse(queryCourseId.FirstOrDefault(), out courseId);
            }

            if (courseId == 0)
            {
                // Can't identify course context, redirect to Home Catalog
                context.Result = new RedirectToActionResult("Index", "Home", null);
                return;
            }

            // Check if enrollment exists in the database
            var hasEnrollment = await _dbContext.Enrollments
                .AnyAsync(e => e.StudentId == userId && e.CourseId == courseId);

            if (!hasEnrollment)
            {
                // Not enrolled! Redirect to the Course Details overview page with a subscription notice
                context.Result = new RedirectToActionResult("Details", "Courses", new { id = courseId, error = "Kursa erişmek için önce kayıt olmalısınız!" });
            }
        }
    }
}
