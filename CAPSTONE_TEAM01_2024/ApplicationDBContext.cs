using CAPSTONE_TEAM01_2024.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CAPSTONE_TEAM01_2024
{
	public class ApplicationDbContext : IdentityDbContext<IdentityUser>
	{
		public ApplicationDbContext(DbContextOptions options) : base(options)
		{
		}
		public DbSet<ProfileManagerModel> ProfileManagers { get; set; }
		public DbSet<AcademicPeriod> AcademicPeriods { get; set; }
		public DbSet<Class> Classes { get; set; }
		public DbSet<Student> Students { get; set; }
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<Class>()
				.HasOne(c => c.Advisor)
				.WithMany()
				.HasForeignKey(c => c.AdvisorId)
				.OnDelete(DeleteBehavior.Restrict); //  // Keeps integrity

			modelBuilder.Entity<ProfileManagerModel>()
				.HasOne(c => c.User)
				.WithMany()
				.HasForeignKey(c => c.UserId);

			modelBuilder.Entity<Student>()
				.HasOne(s => s.ProfileManager)
				.WithMany()
				.HasForeignKey(s => s.ProfileManagerId)
				.OnDelete(DeleteBehavior.Cascade); // Deleting a student from ProfileManager removes them from classes

			modelBuilder.Entity<Student>()
				.HasOne(s => s.Class)
				.WithMany(c => c.Students) // Ensure Class has a collection of Students
				.HasForeignKey(s => s.ClassId)
				.OnDelete(DeleteBehavior.Cascade); // Deleting a class deletes associated students
		}
	}
}
