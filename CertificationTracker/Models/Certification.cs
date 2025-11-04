using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CertificationTracker.Models
{
    public class Certification
    {
        [Key]
        public Guid CertificationId { get; set; }

        [Required, MaxLength(200)]
        public string CertificationName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        public int? ValidityMonths { get; set; }

        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        public virtual ICollection<EmployeeCertification> EmployeeCertifications { get; set; } = new List<EmployeeCertification>();
    }
}