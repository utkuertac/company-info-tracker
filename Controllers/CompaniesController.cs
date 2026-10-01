using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CompanyInfoTracker.Data;
using CompanyInfoTracker.Models;

namespace CompanyInfoTracker.Controllers
{
    [Authorize]
    public class CompaniesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CompaniesController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            const int pageSize = 10;
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Forbid();

            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            IQueryable<Company> query = _context.Companies;

            if (!isAdmin)
            {
                query = query.Where(c => c.Users.Any(u => u.Id == user.Id));
            }

            var totalCount = await query.CountAsync();
            var totalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)pageSize));
            page = Math.Clamp(page, 1, totalPages);

            var companies = await query
                .OrderBy(c => c.Name)
                .ThenBy(c => c.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(companies);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Company company)
        {
            if (!ModelState.IsValid)
                return View(company);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Forbid();

            // Formdan gelebilecek Id / kullanıcı listesini dikkate alma (over-posting koruması)
            company.Id = 0;
            company.Users = new List<ApplicationUser> { user };
            _context.Companies.Add(company);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Firma başarıyla eklendi ve size atandı.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var company = await _context.Companies.FindAsync(id);
            if (company == null) return NotFound();

            return View(company);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Company company)
        {
            if (id != company.Id) return NotFound();

            if (!ModelState.IsValid)
                return View(company);

            var existing = await _context.Companies.FindAsync(id);
            if (existing == null) return NotFound();

            // Sadece formdaki alanları güncelle; kullanıcı atamaları değişmesin
            existing.Name = company.Name;
            existing.VPN = company.VPN;
            existing.Application = company.Application;
            existing.RemoteExternalIP = company.RemoteExternalIP;
            existing.RemoteInternalIP = company.RemoteInternalIP;
            existing.Database = company.Database;

            await _context.SaveChangesAsync();

            TempData["Message"] = "Firma bilgileri güncellendi.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var company = await _context.Companies.FindAsync(id);
            if (company == null) return NotFound();

            return View(company);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var company = await _context.Companies.FindAsync(id);
            if (company != null)
            {
                _context.Companies.Remove(company);
                await _context.SaveChangesAsync();
                TempData["Message"] = "Firma silindi.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
