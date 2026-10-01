using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CompanyInfoTracker.Data;
using CompanyInfoTracker.Models;
using CompanyInfoTracker.ViewModels;

namespace CompanyInfoTracker.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard(int page = 1)
        {
            const int pageSize = 10;

            var totalCompanyCount = await _context.Companies.CountAsync();
            var totalCompanyPages = Math.Max(1, (int)Math.Ceiling(totalCompanyCount / (double)pageSize));
            page = Math.Clamp(page, 1, totalCompanyPages);

            var companies = await _context.Companies
                .Include(c => c.Users)
                .OrderBy(c => c.Name)
                .ThenBy(c => c.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var companyUserData = companies.Select(c => new CompanyAssignmentViewModel
            {
                Company = c,
                AssignedUsers = c.Users.ToList()
            }).ToList();

            ViewBag.AllUsers = await _userManager.Users.OrderBy(u => u.UserName).ToListAsync();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalCompanyPages;

            return View(companyUserData);
        }

        [HttpGet]
        public async Task<IActionResult> UserManagement(int userPage = 1)
        {
            const int pageSize = 10;

            // Her kullanıcı için ayrı rol sorgusu yerine admin listesini tek seferde al
            var adminIds = (await _userManager.GetUsersInRoleAsync("Admin"))
                .Select(u => u.Id)
                .ToList();

            var nonAdminUsers = _userManager.Users.Where(u => !adminIds.Contains(u.Id));

            var totalUserCount = await nonAdminUsers.CountAsync();
            var totalUserPages = Math.Max(1, (int)Math.Ceiling(totalUserCount / (double)pageSize));
            userPage = Math.Clamp(userPage, 1, totalUserPages);

            var pagedUsers = await nonAdminUsers
                .OrderBy(u => u.UserName)
                .Skip((userPage - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.CurrentUserPage = userPage;
            ViewBag.TotalUserPages = totalUserPages;

            return View(pagedUsers);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignUser(int companyId, string userId, int page = 1)
        {
            var company = await _context.Companies
                .Include(c => c.Users)
                .FirstOrDefaultAsync(c => c.Id == companyId);

            var user = await _userManager.FindByIdAsync(userId);

            if (company != null && user != null && !company.Users.Any(u => u.Id == userId))
            {
                company.Users.Add(user);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Dashboard), new { page });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveUser(int companyId, string userId, int page = 1)
        {
            var company = await _context.Companies
                .Include(c => c.Users)
                .FirstOrDefaultAsync(c => c.Id == companyId);

            var user = await _userManager.FindByIdAsync(userId);

            if (company != null && user != null && company.Users.Any(u => u.Id == userId))
            {
                company.Users.Remove(user);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Dashboard), new { page });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                TempData["Error"] = "Kullanıcı bulunamadı.";
            }
            else if (await _userManager.IsInRoleAsync(user, "Admin"))
            {
                TempData["Error"] = "Admin kullanıcılar bu ekrandan silinemez.";
            }
            else
            {
                // Firma atamaları (join tablosu) cascade ile birlikte silinir
                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                    TempData["Message"] = "Kullanıcı başarıyla silindi.";
                else
                    TempData["Error"] = string.Join(" ", result.Errors.Select(e => e.Description));
            }

            return RedirectToAction(nameof(UserManagement));
        }
    }
}
