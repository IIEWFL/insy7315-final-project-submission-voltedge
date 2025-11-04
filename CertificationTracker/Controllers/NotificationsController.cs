using CertificationTracker.Data;
using CertificationTracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CertificationTracker.Controllers
{
    [Authorize(Roles = "HR,Manager")]
    public class NotificationsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IConfiguration _config;
        private readonly ILogger<NotificationsController> _logger;

        public NotificationsController(ApplicationDbContext db, IConfiguration config, ILogger<NotificationsController> logger)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // GET /Notifications
        public async Task<IActionResult> Index()
        {
            try
            {
                var days = _config.GetValue<int>("ExpiryJob:DaysBeforeExpiry", 30);
                var now = DateTime.UtcNow;
                var threshold = now.AddDays(days);

                // Query assignments in window, include Certification and Employee to build the view model
                var expiring = await _db.EmployeeCertifications
                    .AsNoTracking()
                    .Include(ec => ec.Certification)
                    .Include(ec => ec.Employee)
                    .Where(ec => ec.DateExpiry != null && ec.DateExpiry >= now && ec.DateExpiry <= threshold)
                    .ToListAsync();

                var groups = expiring
    .GroupBy(ec => ec.CertificationId)
    .Select(g =>
    {
        var first = g.First();
        var vm = new CertificationNotificationViewModel
        {
            CertificationId = g.Key, // g.Key is Guid (non-nullable)
            CertificationName = first.Certification != null ? first.Certification.CertificationName : "(Unknown)",
            AffectedCount = g.Count(),
            Assignments = g.Select(ec => new CertificationNotificationViewModel.AffectedAssignment
            {
                EmployeeCertificationId = ec.EmployeeCertificationId,
                EmployeeId = ec.EmployeeId,
                EmployeeName = ec.Employee != null ? ec.Employee.FullName : "Unknown",
                DateExpiry = ec.DateExpiry
            }).OrderBy(a => a.DateExpiry).ToList()
        };
        return vm;
    })
    .OrderByDescending(vm => vm.AffectedCount)
    .ToList();

                return View(groups);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load notifications index.");
                TempData["ErrorMessage"] = "There was a problem loading notifications. Please try again later.";
                return View(Enumerable.Empty<CertificationNotificationViewModel>());
            }
        }

        // GET /Notifications/Details/{certificationId}
        public async Task<IActionResult> Details(Guid id)
        {
            if (id == Guid.Empty) return BadRequest();

            try
            {
                var days = _config.GetValue<int>("ExpiryJob:DaysBeforeExpiry", 30);
                var now = DateTime.UtcNow;
                var threshold = now.AddDays(days);

                var assignments = await _db.EmployeeCertifications
                    .AsNoTracking()
                    .Include(ec => ec.Employee)
                    .Include(ec => ec.Certification)
                    .Where(ec => ec.CertificationId == id && ec.DateExpiry != null && ec.DateExpiry >= now && ec.DateExpiry <= threshold)
                    .OrderBy(ec => ec.DateExpiry)
                    .ToListAsync();

                if (!assignments.Any()) return NotFound();

                var vm = new CertificationNotificationViewModel
                {
                    CertificationId = id,
                    CertificationName = assignments.First().Certification?.CertificationName ?? "(Unknown)",
                    AffectedCount = assignments.Count,
                    Assignments = assignments.Select(ec => new CertificationNotificationViewModel.AffectedAssignment
                    {
                        EmployeeCertificationId = ec.EmployeeCertificationId,
                        EmployeeId = ec.EmployeeId,
                        EmployeeName = ec.Employee?.FullName ?? "Unknown",
                        DateExpiry = ec.DateExpiry
                    }).ToList()
                };

                return View(vm);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load notifications details for {Id}", id);
                return StatusCode(500);
            }
        }

        // GET /Notifications/AffectedAssignments/{certificationId}
        public async Task<IActionResult> AffectedAssignments(Guid certificationId)
        {
            if (certificationId == Guid.Empty) return BadRequest();

            try
            {
                var days = _config.GetValue<int>("ExpiryJob:DaysBeforeExpiry", 30);
                var now = DateTime.UtcNow;
                var threshold = now.AddDays(days);

                var items = await _db.EmployeeCertifications
                    .AsNoTracking()
                    .Include(ec => ec.Employee)
                    .Include(ec => ec.Certification)
                    .Where(ec => ec.CertificationId == certificationId
                                 && ec.DateExpiry != null
                                 && ec.DateExpiry >= now
                                 && ec.DateExpiry <= threshold)
                    .OrderBy(ec => ec.DateExpiry)
                    .ToListAsync();

                return View("~/Views/EmployeeCertifications/Index.cshtml", items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load affected assignments for certification {CertId}", certificationId);
                return StatusCode(500);
            }
        }
    }
}