using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Youdemy.Models
{
    public class AdminDashboardViewModel
    {
        public int PendingTeachersCount { get; set; }
        public int TeachersCount { get; set; }
        public int StudentsCount { get; set; }
        public int AllUsersCount { get; set; }
    }

    public class AdminUserCreateViewModel
    {
        [Required(ErrorMessage = "Kullanıcı adı zorunludur.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Kullanıcı adı 3 ile 50 karakter arasında olmalıdır.")]
        [Display(Name = "Kullanıcı Adı")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-posta adresi zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [Display(Name = "E-posta")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre zorunludur.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Rol seçilmelidir.")]
        [Display(Name = "Rol")]
        public UserRole Role { get; set; } = UserRole.Student;

        [Display(Name = "Hesap Onaylı Olarak Oluşturulsun")]
        public bool IsApproved { get; set; } = true;
    }

    public class NotificationSendViewModel
    {
        [Required(ErrorMessage = "Bir hedef grup seçmelisiniz.")]
        [Display(Name = "Hedef Rol")]
        public UserRole RecipientRole { get; set; } = UserRole.Student;

        [Display(Name = "Bireysel Gönderim için Kullanıcılar")]
        public int[]? SelectedRecipientIds { get; set; }

        public IEnumerable<User> AvailableRecipients { get; set; } = new List<User>();

        [Required(ErrorMessage = "Gönderilecek mesaj boş olamaz.")]
        [StringLength(1000, ErrorMessage = "Mesaj 1000 karakterden uzun olamaz.")]
        [Display(Name = "Bildirim Mesajı")]
        public string Message { get; set; } = string.Empty;
    }

    public class TeacherNotificationViewModel
    {
        public IEnumerable<User> AvailableStudents { get; set; } = new List<User>();

        public int? SelectedCourseId { get; set; }

        public string? CourseTitle { get; set; }

        [Display(Name = "Seçili Öğrenciler")]
        public int[]? SelectedStudentIds { get; set; }

        [Required(ErrorMessage = "Gönderilecek mesaj boş olamaz.")]
        [StringLength(1000, ErrorMessage = "Mesaj 1000 karakterden uzun olamaz.")]
        [Display(Name = "Bildirim Mesajı")]
        public string Message { get; set; } = string.Empty;
    }
}
