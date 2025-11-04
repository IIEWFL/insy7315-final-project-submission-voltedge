using CertificationTracker.Data;
using CertificationTracker.Models;
using CertificationTracker.Services;
using CertificationTracker.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace CertificationTracker.Controllers
{
    [Authorize(Roles = "HR,Manager")]
    public class EmployeeCertificationsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IBlobService _blob;
        private readonly string _containerName;

        public EmployeeCertificationsController(ApplicationDbContext db, IBlobService blob, IConfiguration config)
        {
            _db = db;
            _blob = blob;
            _containerName = config.GetValue<string>("Azure:BlobContainerName") ?? "certdocuments";
        }

        public async Task<IActionResult> Index()
        {
            var list = await _db.EmployeeCertifications
                .Include(ec => ec.Employee)
                .Include(ec => ec.Certification)
                .AsNoTracking()
                .ToListAsync();
            return View(list);
        }

        public async Task<IActionResult> Create(Guid? employeeId)
        {
            ViewBag.Employees = await _db.Employees.AsNoTracking().ToListAsync();
            ViewBag.Certifications = await _db.Certifications.AsNoTracking().ToListAsync();
            var vm = new EmployeeCertificationCreateViewModel
            {
                EmployeeId = employeeId ?? Guid.Empty,
                DateIssued = DateTime.UtcNow
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EmployeeCertificationCreateViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Employees = await _db.Employees.AsNoTracking().ToListAsync();
                ViewBag.Certifications = await _db.Certifications.AsNoTracking().ToListAsync();
                return View(vm);
            }

            string? url = null;
            if (vm.Document != null)
            {
                var fileName = $"{Guid.NewGuid()}{System.IO.Path.GetExtension(vm.Document.FileName)}";
                url = await _blob.UploadFileAsync(vm.Document, _containerName, fileName);
            }

            var ec = new EmployeeCertification
            {
                EmployeeCertificationId = Guid.NewGuid(),
                EmployeeId = vm.EmployeeId,
                CertificationId = vm.CertificationId,
                DateIssued = vm.DateIssued,
                DateExpiry = vm.DateExpiry,
                IssuedBy = vm.IssuedBy,
                DocumentUrl = url
            };

            _db.Add(ec);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(Guid id)
        {
            var ec = await _db.EmployeeCertifications
                .Include(e => e.Employee)
                .Include(e => e.Certification)
                .FirstOrDefaultAsync(e => e.EmployeeCertificationId == id);
            if (ec == null) return NotFound();
            return View(ec);
        }

        public async Task<IActionResult> Delete(Guid id)
        {
            var ec = await _db.EmployeeCertifications
                .Include(e => e.Employee)
                .Include(e => e.Certification)
                .FirstOrDefaultAsync(e => e.EmployeeCertificationId == id);
            if (ec == null) return NotFound();
            return View(ec);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var ec = await _db.EmployeeCertifications.FindAsync(id);
            if (ec != null)
            {
                if (!string.IsNullOrEmpty(ec.DocumentUrl))
                {
                    await _blob.DeleteFileAsync(ec.DocumentUrl, _containerName);
                }
                _db.Remove(ec);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}