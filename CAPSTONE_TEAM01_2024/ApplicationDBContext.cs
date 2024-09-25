using CAPSTONE_TEAM01_2024.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CAPSTONE_TEAM01_2024
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }
        public DbSet<ProfileManagerModel> ProfileManagers { get; set; }
    }
}
