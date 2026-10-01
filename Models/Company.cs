using System.Collections.Generic;

namespace CompanyInfoTracker.Models
{
    public class Company
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? VPN { get; set; }

        public ApplicationInfo? Application { get; set; }
        public RemoteAccessInfo? RemoteExternalIP { get; set; }
        public RemoteAccessInfo? RemoteInternalIP { get; set; }
        public DatabaseInfo? Database { get; set; }

        // Çoklu kullanıcı ilişkisi (Many-to-Many)
        public List<ApplicationUser> Users { get; set; } = new();
    }
}
