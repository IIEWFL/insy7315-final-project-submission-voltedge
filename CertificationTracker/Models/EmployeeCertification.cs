using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CertificationTracker.Models
{
    public class EmployeeCertification
    {
        [Key]
        public Guid EmployeeCertificationId { get; set; }

        [Required]
        [ForeignKey(nameof(Employee))]
        public Guid EmployeeId { get; set; }
        public Employee? Employee { get; set; }

        [Required]
        [ForeignKey(nameof(Certification))]
        public Guid CertificationId { get; set; }
        public Certification? Certification { get; set; }

        [Required]
        public DateTime DateIssued { get; set; }

        public DateTime? DateExpiry { get; set; }

        [MaxLength(500)]
        public string? DocumentUrl { get; set; }

        [MaxLength(100)]
        public string? IssuedBy { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}