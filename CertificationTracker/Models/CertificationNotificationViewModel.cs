using System;
using System.Collections.Generic;

namespace CertificationTracker.Models
{
    public class CertificationNotificationViewModel
    {
        public Guid CertificationId { get; set; }
        public string CertificationName { get; set; } = string.Empty;
        public int AffectedCount { get; set; }
        public List<AffectedAssignment> Assignments { get; set; } = new List<AffectedAssignment>();

        public class AffectedAssignment
        {
            public Guid EmployeeCertificationId { get; set; }
            public Guid? EmployeeId { get; set; }
            public string EmployeeName { get; set; } = string.Empty;
            public DateTime? DateExpiry { get; set; }
        }
    }
}