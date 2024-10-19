using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace CAPSTONE_TEAM01_2024.Models
{
	public class ApplicationUser : IdentityUser
    {
        // Additional properties
        public string SchoolId { get; set; }
        public bool IsRegistered { get; set; } = false;
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime? LastLoginTime { get; set; }
    }
}
