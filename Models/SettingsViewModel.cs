using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Youdemy.Models
{
    public class SettingsViewModel
    {
        [Required]
        [StringLength(50, MinimumLength = 3)]
        [Display(Name = "Kullanıcı Adı")]
        public string Username { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "Görünen İsim")]
        public string? DisplayName { get; set; }

        [Display(Name = "Profil Resmi URL")]
        public string? ProfileImageUrl { get; set; }

        [Display(Name = "Profil Resmi Yükle")]
        public IFormFile? ProfileImageFile { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Mevcut Şifre")]
        public string? CurrentPassword { get; set; }

        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6)]
        [Display(Name = "Yeni Şifre")]
        public string? NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Yeni Şifre (Tekrar)")]
        [Compare("NewPassword", ErrorMessage = "Şifreler eşleşmiyor.")]
        public string? ConfirmPassword { get; set; }
    }
}
