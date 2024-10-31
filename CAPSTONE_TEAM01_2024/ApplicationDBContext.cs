﻿using CAPSTONE_TEAM01_2024.Models;
using Humanizer;
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
        public DbSet<AcademicPeriod> AcademicPeriods { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Class> Classes { get; set; }
        public DbSet<Announcement> Announcements { get; set; }
        public DbSet<UserAnnouncement> UserAnnouncements { get; set; }
        public DbSet<SemesterPlan> SemesterPlans { get; set; }
        public DbSet<PlanDetail> PlanDetails { get; set; }
        public DbSet<Criterion> Criterions { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);
            // Application User Table constraints
            modelBuilder.Entity<ApplicationUser>()
                .Property(b => b.IsRegistered)
                .HasDefaultValue(false);

            // Configure one-to-many relationship between ApplicationUser and Class for Advisors
            modelBuilder.Entity<Class>()
                .HasOne(c => c.Advisor)
                .WithMany(u => u.AdvisedClasses)
                .HasForeignKey(c => c.AdvisorId)
                .OnDelete(DeleteBehavior.SetNull); // Ensure AdvisorId can be null and removal of Advisor does not delete Class

            // Configure one-to-many relationship between ApplicationUser and Class for Students
            modelBuilder.Entity<Class>()
                .HasMany(c => c.Students)
                .WithOne(u => u.EnrolledClass)
                .HasForeignKey(u => u.ClassId)
                .OnDelete(DeleteBehavior.Restrict); // Ensure no cascading delete

            modelBuilder.Entity<SemesterPlan>()
                .HasOne(sp => sp.AcademicPeriod)
                .WithMany(ap => ap.SemesterPlans)
                .HasForeignKey(sp => sp.PeriodId);

            modelBuilder.Entity<SemesterPlan>()
                .HasOne(sp => sp.Class)
                .WithMany(c => c.SemesterPlans)
                .HasForeignKey(sp => sp.ClassId);

            modelBuilder.Entity<SemesterPlan>()
                .HasMany(sp => sp.PlanDetails) // A SemesterPlan can have many PlanDetails
                .WithOne(pd => pd.SemesterPlan) // Each PlanDetail belongs to one SemesterPlan
                .HasForeignKey(pd => pd.PlanId) // Foreign key in PlanDetail referencing SemesterPlan
                .OnDelete(DeleteBehavior.Cascade); // If a SemesterPlan is deleted, delete all related PlanDetails

        }
    }
}
