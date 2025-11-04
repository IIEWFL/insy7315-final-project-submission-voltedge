using CertificationTracker.Data;
using CertificationTracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace CertificationTracker.Controllers
{
    [Authorize(Roles = "HR,Manager")]
    public class EmployeesController : Controller
    {
        private readonly ApplicationDbContext _db;

        public EmployeesController(ApplicationDbContext db) { _db = db; }

        public async Task<IActionResult> Index()
        {
            var items = await _db.Employees.AsNoTracking().ToListAsync();
            return View(items);
        }

        public async Task<IActionResult> Details(Guid id)
        {
            var emp = await _db.Employees
                .Include(e => e.EmployeeCertifications)
                .ThenInclude(ec => ec.Certification)
                .FirstOrDefaultAsync(e => e.EmployeeId == id);
            if (emp == null) return NotFound();
            return View(emp);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FullName,Position,Department,DateOfHire")] Employee model)
        {
            if (!ModelState.IsValid) return View(model);
            model.EmployeeId = Guid.NewGuid();
            _db.Add(model);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            var emp = await _db.Employees.FindAsync(id);
            if (emp == null) return NotFound();
            return View(emp);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("EmployeeId,FullName,Position,Department,DateOfHire")] Employee model)
        {
            if (id != model.EmployeeId) return BadRequest();
            if (!ModelState.IsValid) return View(model);
            _db.Update(model);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(Guid id)
        {
            var emp = await _db.Employees.FindAsync(id);
            if (emp == null) return NotFound();
            return View(emp);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var emp = await _db.Employees
                .Include(e => e.EmployeeCertifications)
                .FirstOrDefaultAsync(e => e.EmployeeId == id);
            if (emp != null)
            {
                _db.Remove(emp);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}