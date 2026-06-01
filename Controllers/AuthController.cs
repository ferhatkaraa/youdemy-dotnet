using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Youdemy.Data;
using Youdemy.Helpers;
using Youdemy.Models;

namespace Youdemy.Controllers
{
    public class AuthController : Controller
    {
        private readonly YoudemyDbContext _context;

        public AuthController(YoudemyDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated ?? false)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Prevent creating Admin accounts via public registration
            if (model.Role == UserRole.Admin)
            {
                ModelState.AddModelError("Role", "Admin hesabı kayıt yoluyla oluşturulamaz.");
                return View(model);
            }

            // Check if email already exists
            var emailExists = await _context.Users.AnyAsync(u => u.Email.ToLower() == model.Email.ToLower());
            if (emailExists)
            {
                ModelState.AddModelError("Email", "Bu e-posta adresi zaten kullanımda.");
                return View(model);
            }

            // Check if username already exists
            var usernameExists = await _context.Users.AnyAsync(u => u.Username.ToLower() == model.Username.ToLower());
            if (usernameExists)
            {
                ModelState.AddModelError("Username", "Bu kullanıcı adı zaten alınmış.");
                return View(model);
            }

            // Create user
            var user = new User
            {
                Username = model.Username,
                Email = model.Email.ToLower(),
                PasswordHash = PasswordHelper.HashPassword(model.Password),
                Role = model.Role,
                CreatedAt = DateTime.UtcNow,
                IsApproved = model.Role != UserRole.Teacher
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Auto sign in user after registration
            await SignInUserAsync(user);

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated ?? false)
            {
                return RedirectToAction("Index", "Home");
            }
            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var identifier = model.Identifier?.ToLower() ?? string.Empty;
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == identifier || u.Username.ToLower() == identifier);

            if (user == null || !PasswordHelper.VerifyPassword(model.Password, user.PasswordHash))
            {
                ModelState.AddModelError(string.Empty, "Kullanıcı adı/e-posta veya şifre hatalı.");
                return View(model);
            }

            await SignInUserAsync(user);

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        private async Task SignInUserAsync(User user)
        {
            var claims = new List<Claim>
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

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProperties);
        }
    }
}
