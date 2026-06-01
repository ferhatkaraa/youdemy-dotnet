using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Youdemy.Models
{
    public class LessonProgress
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int StudentId { get; set; }

        [ForeignKey("StudentId")]
        public virtual User? Student { get; set; }

        [Required]
        public int LessonId { get; set; }

        [ForeignKey("LessonId")]
        public virtual Lesson? Lesson { get; set; }

        public bool IsCompleted { get; set; } = false;

        public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
    }
}
