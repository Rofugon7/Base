using System;
using System.ComponentModel.DataAnnotations;

namespace BaseConLogin.Models
{
    public class ContactMessage
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Phone { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Message { get; set; }

        [Required]
        public bool AcceptedPrivacyPolicy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
