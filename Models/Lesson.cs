using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Youdemy.Models
{
    public class Lesson
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Ders başlığı zorunludur.")]
        [StringLength(150, MinimumLength = 3, ErrorMessage = "Ders başlığı en az 3, en fazla 150 karakter olmalıdır.")]
        public string Title { get; set; } = string.Empty;

        [StringLength(2000, ErrorMessage = "Ders açıklaması en fazla 2000 karakter olabilir.")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ders videosu veya linki zorunludur.")]
        public string VideoUrl { get; set; } = string.Empty;

        public int Order { get; set; }

        [Required]
        public int CourseId { get; set; }

        [ForeignKey("CourseId")]
        public virtual Course? Course { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
