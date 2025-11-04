using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;

namespace CertificationTracker.ViewModels
{
    public class EmployeeCertificationCreateViewModel
    {
        [Required]
        public Guid EmployeeId { get; set; }

        [Required]
        public Guid CertificationId { get; set; }

        [Required]
        public DateTime DateIssued { get; set; }

        public DateTime? DateExpiry { get; set; }

        public string? IssuedBy { get; set; }

        public IFormFile? Document { get; set; }
    }
}
