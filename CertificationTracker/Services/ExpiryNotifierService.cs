using CertificationTracker.Data;
using CertificationTracker.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CertificationTracker.Services
{
    public class ExpiryNotifierService : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<ExpiryNotifierService> _logger;
        private readonly IConfiguration _config;
        private readonly TimeSpan _interval;
        private readonly int _daysBeforeExpiry;

        public ExpiryNotifierService(IServiceProvider services, ILogger<ExpiryNotifierService> logger, IConfiguration config)
        {
            _services = services;
            _logger = logger;
            _config = config;
            _interval = TimeSpan.FromMinutes(_config.GetValue<int>("ExpiryJob:IntervalMinutes", 60));
            _daysBeforeExpiry = _config.GetValue<int>("ExpiryJob:DaysBeforeExpiry", 30);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ExpiryNotifierService starting. Interval {minutes} min; DaysBeforeExpiry {days}.", _interval.TotalMinutes, _daysBeforeExpiry);
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckOnce(stoppingToken);
                }
                catch (OperationCanceledException) { /* ignore */ }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in expiry notifier loop");
                }

                await Task.Delay(_interval, stoppingToken);
            }
            _logger.LogInformation("ExpiryNotifierService stopping.");
        }

        private async Task CheckOnce(CancellationToken ct)
        {
            using var scope = _services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var now = DateTime.UtcNow;
            var threshold = now.AddDays(_daysBeforeExpiry);

            // Find assignments expiring within window
            var expiring = await db.EmployeeCertifications
                .AsNoTracking()
                .Include(ec => ec.Employee)
                .Include(ec => ec.Certification)
                .Where(ec => ec.DateExpiry != null && ec.DateExpiry >= now && ec.DateExpiry <= threshold)
                .ToListAsync(ct);

            if (!expiring.Any())
            {
                _logger.LogDebug("No expiring assignments found in this run.");
                return;
            }

            // Group by CertificationId
            var groups = expiring
                .GroupBy(ec => ec.CertificationId)
                .ToList();

            foreach (var g in groups)
            {
                ct.ThrowIfCancellationRequested();

                var certId = g.Key;
                var cert = g.First().Certification; // include ensured Certification loaded
                var certName = cert != null ? cert.CertificationName : "(Unknown)";

                // Build message summarizing affected employees
                var sb = new StringBuilder();
                sb.AppendLine($"The following {g.Count()} assignment(s) for certification '{certName}' are expiring within {_daysBeforeExpiry} day(s):");
                foreach (var ec in g)
                {
                    var employeeName = ec.Employee != null ? ec.Employee.FullName : "Unknown employee";
                    var expiryStr = ec.DateExpiry?.ToString("yyyy-MM-dd") ?? "(no expiry)";
                    sb.AppendLine($"- {employeeName}: expires {expiryStr}");
                }
                var message = sb.ToString();

                // Avoid duplicate notifications for the same certification within the same expiry date window
                var windowKey = now.Date; // daily window; adjust if needed
                var exists = await db.Notifications.AnyAsync(n =>
                    n.CertificationId == certId &&
                    n.Message.Contains($"expiring within {_daysBeforeExpiry} day") &&
                    n.CreatedAt >= now.AddDays(-1), ct); // avoid duplicates created in last day

                if (exists)
                {
                    _logger.LogInformation("Skipping notification for certification {certName} ({id}) because a recent notification exists.", certName, certId);
                    continue;
                }

                var notification = new Notification
                {
                    NotificationId = Guid.NewGuid(),
                    CertificationId = certId,
                    AffectedCount = g.Count(),
                    RepresentativeEmployeeCertificationId = g.First().EmployeeCertificationId,
                    CreatedAt = DateTime.UtcNow,
                    Title = $"Certification expiring soon: {certName}",
                    Message = message,
                    IsRead = false
                };

                db.Notifications.Add(notification);
                await db.SaveChangesAsync(ct);

                _logger.LogInformation("Created notification {nid} for Certification {certName} ({id}) with {count} affected.", notification.NotificationId, certName, certId, notification.AffectedCount);
            }
        }
    }
}