using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using CompanyInfoTracker;
using CompanyInfoTracker.Data;
using CompanyInfoTracker.Models;

// Yerel şifreler repoda değil, git'e girmeyen .env dosyasında tutulur (scripts/setup-env.sh oluşturur)
DotEnv.Load(Path.Combine(Directory.GetCurrentDirectory(), ".env"));

var builder = WebApplication.CreateBuilder(args);

// MVC'yi global olarak Authorize yap
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AuthorizeFilter());
});

// Veritabanı bağlantısı
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "'DefaultConnection' bağlantı dizesi tanımlı değil. README'deki kurulum adımlarına bakın.");
}

// Bağlantı dizesinde şifre yoksa .env'deki SQL Server şifresini ekle
var connectionBuilder = new SqlConnectionStringBuilder(connectionString);
if (string.IsNullOrEmpty(connectionBuilder.Password) && !string.IsNullOrEmpty(builder.Configuration["MSSQL_SA_PASSWORD"]))
{
    connectionBuilder.Password = builder.Configuration["MSSQL_SA_PASSWORD"];
    connectionString = connectionBuilder.ConnectionString;
}

// Güvenlik kilidi: örnek dosyadaki yer tutucularla veya şifresiz bağlantıyla uygulama açılmaz
const string placeholder = "CHANGE_ME";
var secrets = new Dictionary<string, string?>
{
    ["Veritabanı şifresi"] = connectionBuilder.Password,
    ["SeedUsers:AdminPassword"] = builder.Configuration["SeedUsers:AdminPassword"],
    ["SeedUsers:UserPassword"] = builder.Configuration["SeedUsers:UserPassword"],
};
var invalidSecrets = secrets
    .Where(s => s.Value == placeholder || (s.Key == "Veritabanı şifresi" && string.IsNullOrEmpty(s.Value)))
    .Select(s => s.Key)
    .ToList();
if (invalidSecrets.Count > 0)
{
    throw new InvalidOperationException(
        $"Şu değerler eksik veya hâlâ '{placeholder}': {string.Join(", ", invalidSecrets)}. " +
        "Yerelde 'scripts/setup-env.sh' komutunu çalıştırın; sunucuda gerçek değerleri ortam değişkeni veya secret manager ile verin.");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

// Firma şifrelerini şifrelemek için Data Protection; anahtarlar veritabanında saklanır
builder.Services.AddDataProtection()
    .SetApplicationName("CompanyInfoTracker")
    .PersistKeysToDbContext<AppDbContext>();

// Identity yapılandırması
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        options.Lockout.MaxFailedAccessAttempts = 5;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// Cookie yönlendirme ayarları
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

var app = builder.Build();

// Geliştirme ortamı harici hata sayfası
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication(); // kimlik doğrulama
app.UseAuthorization();  // yetkilendirme

// Veritabanını güncelle, ardından kullanıcı ve rolleri oluştur
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await services.GetRequiredService<AppDbContext>().Database.MigrateAsync();

    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var logger = services.GetRequiredService<ILogger<Program>>();
    await SeedData.InitializeAsync(userManager, roleManager, app.Configuration, logger);
}

// Default yönlendirme
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

await app.RunAsync();
