using CAPSTONE_TEAM01_2024.Models;
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
        public DbSet<Email> Emails { get; set; }
        public DbSet<EmailAttachment> EmailAttachments { get; set; }
        public DbSet<EmailRecipient> EmailRecipients { get; set; }
        public DbSet<EmailThread> EmailThreads { get; set; }   
        public DbSet<SemesterReport> SemesterReports { get; set; }      
        public DbSet<CriterionReport> CriterionReports { get; set; }     
        public DbSet<ReportDetail> ReportDetails { get; set; }       
        public DbSet<AttachmentReport> AttachmentReports { get; set; }
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
            
            // Configuring One-to-Many Relationship between Email and EmailRecipient
            modelBuilder.Entity<EmailRecipient>()
                .HasOne(er => er.Email)
                .WithMany(e => e.Recipients)
                .HasForeignKey(er => er.EmailId)
                .OnDelete(DeleteBehavior.Cascade); // Optional: Cascade delete if an Email is deleted

            // Configuring Many-to-One Relationship between EmailRecipient and ApplicationUser
            modelBuilder.Entity<EmailRecipient>()
                .HasOne(er => er.User)
                .WithMany()
                .HasForeignKey(er => er.UserId)
                .OnDelete(DeleteBehavior.Restrict); // Optional: Prevent deleting ApplicationUser if they are in EmailRecipients

            // Configuring One-to-Many Relationship between Email and EmailAttachment
            modelBuilder.Entity<EmailAttachment>()
                .HasOne(ea => ea.Email)
                .WithMany(e => e.Attachments)
                .HasForeignKey(ea => ea.EmailId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configuring Many-to-One Relationship between Email and EmailThread
            modelBuilder.Entity<Email>()
                .HasOne(e => e.Thread)
                .WithMany(t => t.Emails)
                .HasForeignKey(e => e.ThreadId)
                .OnDelete(DeleteBehavior.SetNull); // Optional: Set ThreadId to null if Email is deleted

            // Configuring EmailThread (No foreign keys, just a collection)
            modelBuilder.Entity<EmailThread>()
                .HasMany(t => t.Emails)
                .WithOne(e => e.Thread)
                .HasForeignKey(e => e.ThreadId)
                .OnDelete(DeleteBehavior.SetNull); // Optional: Set EmailThread to null if an Email is deleted
            //
            modelBuilder.Entity<SemesterReport>()
                .HasOne(sr => sr.AcademicPeriod)
                .WithMany(ap => ap.SemesterReports)
                .HasForeignKey(sr => sr.PeriodId);
            
            modelBuilder.Entity<SemesterReport>()
                .HasOne(sr => sr.Class)
                .WithMany(c => c.SemesterReports)
                .HasForeignKey(sr => sr.ClassId);
            
            modelBuilder.Entity<SemesterReport>()
                .HasMany(sp=>sp.ReportDetails)
                .WithOne(r => r.SemesterReport)
                .HasForeignKey(r=>r.ReportId)
                .OnDelete(DeleteBehavior.Cascade);
            
            modelBuilder.Entity<ReportDetail>()
                .HasOne(rd => rd.CriterionReport)
                .WithMany()
                .HasForeignKey(rd => rd.CriterionId)
                .OnDelete(DeleteBehavior.Restrict);
            
            modelBuilder.Entity<AttachmentReport>()
                .HasOne(ar => ar.DetailReport) 
                .WithMany(rd => rd.AttachmentReport) 
                .HasForeignKey(ar => ar.DetailReportlId)
                .OnDelete(DeleteBehavior.Cascade); 
            
           

        }
    }
}
