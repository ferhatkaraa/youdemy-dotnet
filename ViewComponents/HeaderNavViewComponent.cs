using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Youdemy.Data;

namespace Youdemy.ViewComponents
{
    public class HeaderNavViewComponent : ViewComponent
    {
        private readonly YoudemyDbContext _context;

        public HeaderNavViewComponent(YoudemyDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var user = HttpContext.User;
            var isAuthenticated = user.Identity?.IsAuthenticated ?? false;

            var viewModel = new HeaderNavViewModel
            {
                IsAuthenticated = isAuthenticated
            };

            if (isAuthenticated)
            {
                viewModel.Role = user.FindFirst(ClaimTypes.Role)?.Value;
                viewModel.UserName = user.Identity?.Name;
                viewModel.DisplayName = user.FindFirst("displayName")?.Value;
                viewModel.ProfileImage = user.FindFirst("profileImage")?.Value;

                var idClaim = user.FindFirst(ClaimTypes.NameIdentifier);
                if (idClaim != null && int.TryParse(idClaim.Value, out var userId))
                {
                    viewModel.UserId = userId;
                    viewModel.UnreadNotificationsCount = await _context.Notifications
                        .CountAsync(n => n.RecipientId == userId && !n.IsRead);
                }
            }

            return View(viewModel);
        }
    }

    public class HeaderNavViewModel
    {
        public bool IsAuthenticated { get; set; }
        public string? Role { get; set; }
        public int UserId { get; set; }
        public int UnreadNotificationsCount { get; set; }
        public string? ProfileImage { get; set; }
        public string? DisplayName { get; set; }
        public string? UserName { get; set; }
    }
}
