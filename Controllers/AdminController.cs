using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Youdemy.Data;
using Youdemy.Helpers;
using Youdemy.Models;

namespace Youdemy.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly YoudemyDbContext _context;

        public AdminController(YoudemyDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var pendingTeachersCount = await _context.Users
                .CountAsync(u => u.Role == UserRole.Teacher && !u.IsApproved);

            var teachersCount = await _context.Users
                .CountAsync(u => u.Role == UserRole.Teacher && u.IsApproved);

            var studentsCount = await _context.Users
                .CountAsync(u => u.Role == UserRole.Student);

            var allUsersCount = await _context.Users.CountAsync();

            var model = new AdminDashboardViewModel
            {
                PendingTeachersCount = pendingTeachersCount,
                TeachersCount = teachersCount,
                StudentsCount = studentsCount,
                AllUsersCount = allUsersCount
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers(string search = "", string role = "PendingTeacher", int skip = 0, int take = 20)
        {
            var query = _context.Users.AsQueryable();

            if (role != "All")
            {
                if (role == "PendingTeacher")
                {
                    query = query.Where(u => u.Role == UserRole.Teacher && !u.IsApproved);
                }
                else if (role == "Teacher")
                {
                    query = query.Where(u => u.Role == UserRole.Teacher && u.IsApproved);
                }
                else if (Enum.TryParse<UserRole>(role, out var parsedRole))
                {
                    query = query.Where(u => u.Role == parsedRole);
                }
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                query = query.Where(u => u.Username.ToLower().Contains(search) || u.Email.ToLower().Contains(search));
            }

            var totalCount = await query.CountAsync();
            var users = await query.OrderByDescending(u => u.CreatedAt)
                                   .Skip(skip)
                                   .Take(take)
                                   .Select(u => new {
                                       id = u.Id,
                                       username = u.Username,
                                       email = u.Email,
                                       role = u.Role.ToString(),
                                       isApproved = u.IsApproved,
                                       createdAt = u.CreatedAt.ToString("dd.MM.yyyy HH:mm")
                                   })
                                   .ToListAsync();

            return Json(new { users, totalCount });
        }

        public IActionResult CreateUser()
        {
            return View(new AdminUserCreateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(AdminUserCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var emailExists = await _context.Users.AnyAsync(u => u.Email.ToLower() == model.Email.ToLower());
            if (emailExists)
            {
                ModelState.AddModelError("Email", "Bu e-posta adresi zaten kullanımda.");
                return View(model);
            }

            var usernameExists = await _context.Users.AnyAsync(u => u.Username.ToLower() == model.Username.ToLower());
            if (usernameExists)
            {
                ModelState.AddModelError("Username", "Bu kullanıcı adı zaten alınmış.");
                return View(model);
            }

            var user = new User
            {
                Username = model.Username,
                Email = model.Email.ToLower(),
                PasswordHash = PasswordHelper.HashPassword(model.Password),
                Role = model.Role,
                IsApproved = model.Role != UserRole.Teacher || model.IsApproved,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Kullanıcı başarıyla oluşturuldu.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveTeacher(int id)
        {
            var teacher = await _context.Users.FirstOrDefaultAsync(u => u.Id == id && u.Role == UserRole.Teacher);
            if (teacher == null)
            {
                return NotFound();
            }

            teacher.IsApproved = true;
            _context.Entry(teacher).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Öğretmen hesabı onaylandı.";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> SendNotification(UserRole recipientRole = UserRole.Student)
        {
            var recipients = await _context.Users
                .Where(u => u.Role == recipientRole)
                .OrderBy(u => u.Username)
                .ToListAsync();

            var model = new NotificationSendViewModel
            {
                RecipientRole = recipientRole,
                AvailableRecipients = recipients
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendNotification(NotificationSendViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.AvailableRecipients = await _context.Users
                    .Where(u => u.Role == model.RecipientRole)
                    .OrderBy(u => u.Username)
                    .ToListAsync();
                return View(model);
            }

            var senderId = GetUserId();
            var recipientsQuery = _context.Users.Where(u => u.Role == model.RecipientRole);
            if (model.SelectedRecipientIds != null && model.SelectedRecipientIds.Length > 0)
            {
                recipientsQuery = recipientsQuery.Where(u => model.SelectedRecipientIds.Contains(u.Id));
            }

            var recipients = await recipientsQuery.ToListAsync();
            foreach (var recipient in recipients)
            {
                _context.Notifications.Add(new Notification
                {
                    RecipientId = recipient.Id,
                    SenderId = senderId,
                    Message = model.Message,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"{recipients.Count} kullanıcıya bildirim gönderildi.";
            return RedirectToAction("Index");
        }

        private int GetUserId()
        {
            var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            return claim != null ? int.Parse(claim.Value) : 0;
        }
    }
}
