using CertificationTracker.Data;
using CertificationTracker.Models;
using CertificationTracker.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace CertificationTracker.Controllers
{
    [Authorize(Roles = "HR,Manager")]
    public class CertificationsController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IBlobService _blob;
        private readonly string _containerName;

        public CertificationsController(ApplicationDbContext db, IBlobService blob, IConfiguration config)
        {
            _db = db;
            _blob = blob;
            _containerName = config.GetValue<string>("Azure:BlobContainerName") ?? "certdocuments";
        }

        public async Task<IActionResult> Index()
        {
            var list = await _db.Certifications.AsNoTracking().ToListAsync();
            return View(list);
        }

        public async Task<IActionResult> Details(Guid id)
        {
            var cert = await _db.Certifications
                .Include(c => c.EmployeeCertifications)
                .ThenInclude(ec => ec.Employee)
                .FirstOrDefaultAsync(c => c.CertificationId == id);
            if (cert == null) return NotFound();
            return View(cert);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CertificationName,Description,ValidityMonths")] Certification model, IFormFile? ImageFile)
        {
            if (!ModelState.IsValid) return View(model);

            model.CertificationId = Guid.NewGuid();

            if (ImageFile != null)
            {
                var fileName = $"cert-{model.CertificationId}{Path.GetExtension(ImageFile.FileName)}";
                model.ImageUrl = await _blob.UploadFileAsync(ImageFile, _containerName, fileName);
            }

            _db.Add(model);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            var cert = await _db.Certifications.FindAsync(id);
            if (cert == null) return NotFound();
            return View(cert);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("CertificationId,CertificationName,Description,ValidityMonths,ImageUrl")] Certification model, IFormFile? ImageFile)
        {
            if (id != model.CertificationId) return BadRequest();
            if (!ModelState.IsValid) return View(model);

            if (ImageFile != null)
            {
                // delete previous image if exists
                if (!string.IsNullOrEmpty(model.ImageUrl))
                {
                    try
                    {
                        await _blob.DeleteFileAsync(model.ImageUrl, _containerName);
                    }
                    catch { /* non-fatal if deletion fails */ }
                }
                var fileName = $"cert-{model.CertificationId}{Path.GetExtension(ImageFile.FileName)}";
                model.ImageUrl = await _blob.UploadFileAsync(ImageFile, _containerName, fileName);
            }

            _db.Update(model);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(Guid id)
        {
            var cert = await _db.Certifications.FindAsync(id);
            if (cert == null) return NotFound();
            return View(cert);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var cert = await _db.Certifications.FindAsync(id);
            if (cert != null)
            {
                // remove image from blob if present
                if (!string.IsNullOrEmpty(cert.ImageUrl))
                {
                    try
                    {
                        await _blob.DeleteFileAsync(cert.ImageUrl, _containerName);
                    }
                    catch { /* ignore deletion failures */ }
                }

                _db.Remove(cert);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}