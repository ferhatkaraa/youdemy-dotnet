using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Youdemy.Models
{
    public class Course
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Kurs başlığı zorunludur.")]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "Kurs başlığı en az 5, en fazla 100 karakter olmalıdır.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kurs açıklaması zorunludur.")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Kurs açıklaması en az 10, en fazla 1000 karakter olmalıdır.")]
        public string Description { get; set; } = string.Empty;

        public string ImageUrl { get; set; } = "/images/course-default.png";

        [Required]
        public int TeacherId { get; set; }

        [ForeignKey("TeacherId")]
        public virtual User? Teacher { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
        public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    }
}
