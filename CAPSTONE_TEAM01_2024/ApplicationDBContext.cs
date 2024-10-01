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
        public DbSet<AcademicPeriod> AcademicPeriods { get; set; }
        public DbSet<Class> Classes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
			base.OnModelCreating(modelBuilder);
			modelBuilder.Entity<Class>()
                .HasOne(c => c.Advisor)
                .WithMany()
                .HasForeignKey(c => c.AdvisorId);
        }
    }
}
