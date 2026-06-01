using System;
using System.ComponentModel.DataAnnotations;

namespace Youdemy.Models
{
    public class CourseCreateEditViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Kurs başlığı zorunludur.")]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "Kurs başlığı 5 ile 100 karakter arasında olmalıdır.")]
        [Display(Name = "Kurs Başlığı")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kurs açıklaması zorunludur.")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "Kurs açıklaması 10 ile 1000 karakter arasında olmalıdır.")]
        [Display(Name = "Kurs Açıklaması")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Kapak Görseli (İsteğe Bağlı)")]
        public string? ImageUrl { get; set; }
    }

    public class StudentCourseProgressViewModel
    {
        public Course Course { get; set; } = null!;
        public DateTime EnrolledAt { get; set; }
        public int TotalLessonsCount { get; set; }
        public int CompletedLessonsCount { get; set; }
        public double ProgressPercentage { get; set; }
    }

    public class CourseWatchViewModel
    {
        public Course Course { get; set; } = null!;
        public Lesson ActiveLesson { get; set; } = null!;
        public int CompletedLessonsCount { get; set; }
        public int TotalLessonsCount { get; set; }
        public double ProgressPercentage { get; set; }
    }
}
