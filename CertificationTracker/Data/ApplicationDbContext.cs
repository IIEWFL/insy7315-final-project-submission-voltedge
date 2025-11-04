using CertificationTracker.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace CertificationTracker.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Employee> Employees { get; set; }
        public DbSet<Certification> Certifications { get; set; }
        public DbSet<EmployeeCertification> EmployeeCertifications { get; set; }
        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<Notification> Notifications { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Employee>(b =>
            {
                b.HasKey(e => e.EmployeeId);
                b.HasIndex(e => e.FullName);
            });

            modelBuilder.Entity<Certification>(b =>
            {
                b.HasKey(c => c.CertificationId);
                b.HasIndex(c => c.CertificationName);
            });

            modelBuilder.Entity<EmployeeCertification>(b =>
            {
                b.HasKey(ec => ec.EmployeeCertificationId);
                b.HasOne(ec => ec.Employee)
                    .WithMany(e => e.EmployeeCertifications)
                    .HasForeignKey(ec => ec.EmployeeId)
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasOne(ec => ec.Certification)
                    .WithMany(c => c.EmployeeCertifications)
                    .HasForeignKey(ec => ec.CertificationId)
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasIndex(ec => new { ec.EmployeeId, ec.CertificationId });
            });

            modelBuilder.Entity<AppUser>(b =>
            {
                b.HasKey(u => u.AppUserId);
                b.HasIndex(u => u.Username).IsUnique();
                b.HasOne<Employee>()
                 .WithMany()
                 .HasForeignKey(u => u.EmployeeId)
                 .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Notification>(b =>
            {
                b.HasKey(n => n.NotificationId);
                b.Property(n => n.Title).HasMaxLength(200);
                b.Property(n => n.Message).HasMaxLength(2000);
                b.HasIndex(n => n.CertificationId);
            });


        }
    }
}