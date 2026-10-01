using Microsoft.AspNetCore.Identity;
using CompanyInfoTracker.Models;

namespace CompanyInfoTracker.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration,
            ILogger logger)
        {
            // Roller varsa geç
            if (!await roleManager.RoleExistsAsync("Admin"))
                await roleManager.CreateAsync(new IdentityRole("Admin"));

            if (!await roleManager.RoleExistsAsync("Kullanici"))
                await roleManager.CreateAsync(new IdentityRole("Kullanici"));

            // Şifreler koda gömülü değil; yapılandırmadan (user-secrets / ortam değişkeni) okunur
            await EnsureUserAsync(userManager, logger, "admin", "Admin",
                configuration["SeedUsers:AdminPassword"]);
            await EnsureUserAsync(userManager, logger, "kullanici", "Kullanici",
                configuration["SeedUsers:UserPassword"]);
        }

        private static async Task EnsureUserAsync(
            UserManager<ApplicationUser> userManager,
            ILogger logger,
            string userName,
            string role,
            string? password)
        {
            if (await userManager.FindByNameAsync(userName) != null)
                return;

            if (string.IsNullOrWhiteSpace(password))
            {
                logger.LogWarning("'{UserName}' kullanıcısı için şifre tanımlı değil, oluşturulmadı.", userName);
                return;
            }

            var user = new ApplicationUser { UserName = userName };
            var result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                logger.LogError("'{UserName}' kullanıcısı oluşturulamadı: {Errors}", userName,
                    string.Join(", ", result.Errors.Select(e => e.Description)));
                return;
            }

            await userManager.AddToRoleAsync(user, role);
        }
    }
}
