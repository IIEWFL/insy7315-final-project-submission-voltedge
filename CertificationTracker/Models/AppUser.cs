using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CertificationTracker.Models
{
    public class AppUser
    {
        [Key]
        public Guid AppUserId { get; set; }

        [Required, MaxLength(256)]
        public string Username { get; set; } = string.Empty;

        [Required]
        public byte[] PasswordHash { get; set; } = Array.Empty<byte>();

        [Required]
        public byte[] PasswordSalt { get; set; } = Array.Empty<byte>();

        [Required, MaxLength(50)]
        public string Role { get; set; } = "HR";

        public Guid? EmployeeId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}