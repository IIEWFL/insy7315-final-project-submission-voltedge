using CertificationTracker.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace CertificationTracker.ViewComponents
{
    public class NotificationsBadgeViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _db;
        private readonly IConfiguration _config;
        private readonly ILogger<NotificationsBadgeViewComponent> _logger;

        public NotificationsBadgeViewComponent(ApplicationDbContext db, IConfiguration config, ILogger<NotificationsBadgeViewComponent> logger)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            try
            {
                var days = _config.GetValue<int>("ExpiryJob:DaysBeforeExpiry", 30);
                var now = DateTime.UtcNow;
                var threshold = now.AddDays(days);

                // Count distinct certifications that have expiring assignments
                var count = await _db.EmployeeCertifications
                    .AsNoTracking()
                    .Where(ec => ec.DateExpiry != null && ec.DateExpiry >= now && ec.DateExpiry <= threshold)
                    .Select(ec => ec.CertificationId)
                    .Distinct()
                    .CountAsync();

                return View(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to compute notifications badge.");
                return View(0);
            }
        }
    }
}