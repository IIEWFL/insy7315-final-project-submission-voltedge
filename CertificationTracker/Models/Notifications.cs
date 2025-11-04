using System;
using System.ComponentModel.DataAnnotations;

namespace CertificationTracker.Models
{
    public class Notification
    {
        [Key]
        public Guid NotificationId { get; set; }

        // Primary entity this notification is about: Certification
        public Guid? CertificationId { get; set; }

        // How many employee assignments are affected (for quick summary)
        public int AffectedCount { get; set; }

        // Optionally record one representative EmployeeCertificationId (or null)
        public Guid? RepresentativeEmployeeCertificationId { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required, MaxLength(2000)]
        public string Message { get; set; } = string.Empty;

        public bool IsRead { get; set; } = false;
    }
}
