using CompanyInfoTracker.Models;
using System.Collections.Generic;

namespace CompanyInfoTracker.ViewModels
{
    public class CompanyAssignmentViewModel
    {
        public Company Company { get; set; } = null!;
        public List<ApplicationUser> AssignedUsers { get; set; } = new();
    }
}
