using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CompanyInfoTracker.Models;

namespace CompanyInfoTracker.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>, IDataProtectionKeyContext
    {
        private readonly IDataProtectionProvider _dataProtectionProvider;

        public AppDbContext(DbContextOptions<AppDbContext> options, IDataProtectionProvider dataProtectionProvider)
            : base(options)
        {
            _dataProtectionProvider = dataProtectionProvider;
        }

        public DbSet<Company> Companies { get; set; }

        // Data Protection anahtarları (firma şifrelerini çözebilmek için veritabanında saklanır)
        public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Firma şifreleri veritabanında şifreli tutulur
            var passwordConverter = new ProtectedStringConverter(
                _dataProtectionProvider.CreateProtector("CompanyInfoTracker.CompanyPasswords"));

            modelBuilder.Entity<Company>().OwnsOne(c => c.Application,
                o => o.Property(p => p.Password).HasConversion(passwordConverter));
            modelBuilder.Entity<Company>().OwnsOne(c => c.RemoteExternalIP,
                o => o.Property(p => p.Password).HasConversion(passwordConverter));
            modelBuilder.Entity<Company>().OwnsOne(c => c.RemoteInternalIP,
                o => o.Property(p => p.Password).HasConversion(passwordConverter));
            modelBuilder.Entity<Company>().OwnsOne(c => c.Database,
                o => o.Property(p => p.Password).HasConversion(passwordConverter));

            modelBuilder.Entity<Company>()
                .HasMany(c => c.Users)
                .WithMany(u => u.Companies);
        }
    }
}
