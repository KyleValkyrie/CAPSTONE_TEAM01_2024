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
        public DbSet<AcademicPeriod> AcademicPeriods { get; set; }
        public DbSet<Class> Classes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AcademicPeriod>()
                .HasMany(ap => ap.Classes)
                .WithOne(c => c.AcademicPeriod)
                .HasForeignKey(c => c.AcademicPeriodId);
        }
    }
}
