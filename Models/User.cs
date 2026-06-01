using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Youdemy.Models
{
    public enum UserRole
    {
        Student,
        Teacher,
        Admin
    }

    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        public UserRole Role { get; set; } = UserRole.Student;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Optional display name and profile image URL for personalization
        [StringLength(100)]
        public string? DisplayName { get; set; }

        [Url]
        public string? ProfileImageUrl { get; set; } = "/images/user-default.png";

        public bool IsApproved { get; set; } = true;

        // Navigation properties
        public virtual ICollection<Course> CreatedCourses { get; set; } = new List<Course>();
        public virtual ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
        public virtual ICollection<LessonProgress> LessonProgresses { get; set; } = new List<LessonProgress>();
    }
}
