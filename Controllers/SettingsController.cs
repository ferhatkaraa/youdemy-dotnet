using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Youdemy.Data;
using Youdemy.Helpers;
using Youdemy.Models;

namespace Youdemy.Controllers
{
    [Authorize]
    public class SettingsController : Controller
    {
        private readonly YoudemyDbContext _context;

        public SettingsController(YoudemyDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Challenge();

            var user = await _context.Users.FindAsync(int.Parse(userId));
            if (user == null) return NotFound();

            var model = new SettingsViewModel
            {
                Username = user.Username,
                DisplayName = user.DisplayName,
                ProfileImageUrl = user.ProfileImageUrl
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SettingsViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Challenge();

            var user = await _context.Users.FindAsync(int.Parse(userId));
            if (user == null) return NotFound();

            // Username uniqueness
            if (!string.Equals(user.Username, model.Username, System.StringComparison.OrdinalIgnoreCase))
            {
                var exists = await _context.Users.AnyAsync(u => u.Username.ToLower() == model.Username.ToLower() && u.Id != user.Id);
                if (exists)
                {
                    ModelState.AddModelError("Username", "Bu kullanıcı adı zaten alınmış.");
                    return View(model);
                }
            }

            // Handle password change if requested
            if (!string.IsNullOrEmpty(model.NewPassword))
            {
                if (string.IsNullOrEmpty(model.CurrentPassword) || !PasswordHelper.VerifyPassword(model.CurrentPassword, user.PasswordHash))
                {
                    ModelState.AddModelError("CurrentPassword", "Mevcut şifre geçerli değil.");
                    return View(model);
                }

                user.PasswordHash = PasswordHelper.HashPassword(model.NewPassword);
            }

            user.Username = model.Username;
            user.DisplayName = model.DisplayName;

            // Handle profile picture upload
            if (model.ProfileImageFile != null && model.ProfileImageFile.Length > 0)
            {
                var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "profiles");
                if (!Directory.Exists(uploadsDir))
                {
                    Directory.CreateDirectory(uploadsDir);
                }
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ProfileImageFile.FileName);
                var filePath = Path.Combine(uploadsDir, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ProfileImageFile.CopyToAsync(stream);
                }
                user.ProfileImageUrl = "/uploads/profiles/" + fileName;
            }
            else if (!string.IsNullOrEmpty(model.ProfileImageUrl))
            {
                user.ProfileImageUrl = model.ProfileImageUrl;
            }

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            // Re-sign-in to refresh claims (username, displayName, profileImage)
            await ReSignInAsync(user);

            TempData["SuccessMessage"] = "Ayarlarınız güncellendi.";
            return RedirectToAction("Edit");
        }

        private async System.Threading.Tasks.Task ReSignInAsync(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim("displayName", user.DisplayName ?? string.Empty),
                new Claim("profileImage", user.ProfileImageUrl ?? string.Empty)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
        }
    }
}
