using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace CompanyInfoTracker.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Yeni kullanıcı bilgileri
        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        // Many-to-many firma ilişkisi
        public List<Company> Companies { get; set; } = new();
    }
}
