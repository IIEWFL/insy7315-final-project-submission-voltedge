using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CertificationTracker.Models
{
    public class Employee
    {
        [Key]
        public Guid EmployeeId { get; set; }

        [Required, MaxLength(200)]
        public string FullName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Position { get; set; } = string.Empty;

        [MaxLength(100)]
        public string Department { get; set; } = string.Empty;

        public DateTime? DateOfHire { get; set; }

        public virtual ICollection<EmployeeCertification> EmployeeCertifications { get; set; } = new List<EmployeeCertification>();
    }
}