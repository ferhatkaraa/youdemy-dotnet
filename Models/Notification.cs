using System;
using System.ComponentModel.DataAnnotations;

namespace Youdemy.Models
{
    public class Notification
    {
        [Key]
        public int Id { get; set; }

        public int RecipientId { get; set; }
        public User? Recipient { get; set; }

        public int? SenderId { get; set; }
        public User? Sender { get; set; }

        [Required]
        [StringLength(1000, ErrorMessage = "Bildirim mesajı en fazla 1000 karakter olabilir.")]
        public string Message { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsRead { get; set; } = false;
    }
}
