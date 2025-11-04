using CertificationTracker.Helpers;
using CertificationTracker.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CertificationTracker.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var sp = scope.ServiceProvider;
            var db = sp.GetRequiredService<ApplicationDbContext>();

            // Apply Migrations
            await db.Database.MigrateAsync();

            if (db.AppUsers.Any()) return; // already seeded

            // Seed employees and certifications minimal test data
            var emp1 = new Employee { EmployeeId = Guid.NewGuid(), FullName = "John Doe", Position = "Operator", Department = "Mining", DateOfHire = new DateTime(2018, 5, 1) };
            var emp2 = new Employee { EmployeeId = Guid.NewGuid(), FullName = "Jane Smith", Position = "Supervisor", Department = "Operations", DateOfHire = new DateTime(2016, 3, 15) };
            db.Employees.AddRange(emp1, emp2);

            var cert1 = new Certification { CertificationId = Guid.NewGuid(), CertificationName = "Safety Induction", Description = "General safety induction", ValidityMonths = 12 };
            var cert2 = new Certification { CertificationId = Guid.NewGuid(), CertificationName = "First Aid", Description = "First aid training", ValidityMonths = 24 };
            db.Certifications.AddRange(cert1, cert2);

            var ec = new EmployeeCertification
            {
                EmployeeCertificationId = Guid.NewGuid(),
                EmployeeId = emp1.EmployeeId,
                CertificationId = cert1.CertificationId,
                DateIssued = DateTime.UtcNow.AddMonths(-6),
                DateExpiry = DateTime.UtcNow.AddMonths(6),
                IssuedBy = "HR Admin"
            };
            db.EmployeeCertifications.Add(ec);

            // Create AppUsers (HR and Manager) - password will be hashed with PBKDF2
            PasswordHasher.CreatePasswordHash("Pa$$word123!", out var hrHash, out var hrSalt);
            var hrUser = new AppUser
            {
                AppUserId = Guid.NewGuid(),
                Username = "hr.admin@company.local",
                PasswordHash = hrHash,
                PasswordSalt = hrSalt,
                Role = "HR",
                EmployeeId = null
            };

            PasswordHasher.CreatePasswordHash("Pa$$word123!", out var mgrHash, out var mgrSalt);
            var mgrUser = new AppUser
            {
                AppUserId = Guid.NewGuid(),
                Username = "manager@company.local",
                PasswordHash = mgrHash,
                PasswordSalt = mgrSalt,
                Role = "Manager",
                EmployeeId = emp2.EmployeeId
            };

            db.AppUsers.AddRange(hrUser, mgrUser);

            await db.SaveChangesAsync();
        }
    }
}