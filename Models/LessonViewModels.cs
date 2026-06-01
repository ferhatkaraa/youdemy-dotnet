using System.ComponentModel.DataAnnotations;

namespace Youdemy.Models
{
    public class LessonCreateEditViewModel
    {
        public int Id { get; set; }

        [Required]
        public int CourseId { get; set; }

        public string? CourseTitle { get; set; }

        [Required(ErrorMessage = "Ders başlığı zorunludur.")]
        [StringLength(150, MinimumLength = 3, ErrorMessage = "Ders başlığı 3 ile 150 karakter arasında olmalıdır.")]
        [Display(Name = "Ders Başlığı")]
        public string Title { get; set; } = string.Empty;

        [StringLength(2000, ErrorMessage = "Ders açıklaması en fazla 2000 karakter olabilir.")]
        [Display(Name = "Ders Açıklaması")]
        public string? Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ders video URL'si veya gömme kodu zorunludur.")]
        [Display(Name = "Video URL (Örn: Youtube Embed URL)")]
        public string VideoUrl { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ders sırası zorunludur.")]
        [Display(Name = "Ders Sırası")]
        public int Order { get; set; } = 1;
    }
}
