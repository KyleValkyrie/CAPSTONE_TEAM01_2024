﻿// <auto-generated />
using System;
using CAPSTONE_TEAM01_2024;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace CAPSTONE_TEAM01_2024.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20250118122759_RemoveNotificationTable")]
    partial class RemoveNotificationTable
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.33")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder, 1L, 1);

            modelBuilder.Entity("CAPSTONE_TEAM01_2024.Models.AcademicPeriod", b =>
                {
                    b.Property<int>("PeriodId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("PeriodId"), 1L, 1);

                    b.Property<DateTime>("PeriodEnd")
                        .HasColumnType("datetime2");

                    b.Property<string>("PeriodName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("PeriodStart")
                        .HasColumnType("datetime2");

                    b.HasKey("PeriodId");

                    b.ToTable("AcademicPeriods");
                });

            modelBuilder.Entity("CAPSTONE_TEAM01_2024.Models.Announcement", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<string>("Detail")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("EndTime")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("StartTime")
                        .HasColumnType("datetime2");

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Announcements");
                });

            modelBuilder.Entity("CAPSTONE_TEAM01_2024.Models.AttachmentReport", b =>
                {
                    b.Property<int>("AttachmentReportId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("AttachmentReportId"), 1L, 1);

                    b.Property<int>("DetailReportlId")
                        .HasColumnType("int");

                    b.Property<byte[]>("FileDatas")
                        .IsRequired()
                        .HasColumnType("varbinary(max)");

                    b.Property<string>("FileNames")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("AttachmentReportId");

                    b.HasIndex("DetailReportlId");

                    b.ToTable("AttachmentReports");
                });

            modelBuilder.Entity("CAPSTONE_TEAM01_2024.Models.Class", b =>
                {
                    b.Property<string>("ClassId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("AdvisorId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Department")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("StudentCount")
                        .HasColumnType("int");

                    b.Property<string>("Term")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("ClassId");

                    b.HasIndex("AdvisorId");

                    b.ToTable("Classes");
                });

            modelBuilder.Entity("CAPSTONE_TEAM01_2024.Models.Criterion", b =>
                {
                    b.Property<int>("CriterionId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("CriterionId"), 1L, 1);

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("CriterionId");

                    b.ToTable("Criterions");
                });

            modelBuilder.Entity("CAPSTONE_TEAM01_2024.Models.CriterionReport", b =>
                {
                    b.Property<int>("CriterionId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("CriterionId"), 1L, 1);

                    b.Property<string>("DescriptionReport")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("NameReport")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("CriterionId");

                    b.ToTable("CriterionReports");
                });

            modelBuilder.Entity("CAPSTONE_TEAM01_2024.Models.Email", b =>
                {
                    b.Property<int>("EmailId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("EmailId"), 1L, 1);

                    b.Property<string>("Content")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("SenderId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTime>("SentDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Subject")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("ThreadId")
                        .IsRequired()
                        .HasColumnType("int");

                    b.HasKey("EmailId");

                    b.HasIndex("SenderId");

                    b.HasIndex("ThreadId");

                    b.ToTable("Emails");
                });

            modelBuilder.Entity("CAPSTONE_TEAM01_2024.Models.EmailAttachment", b =>
                {
                    b.Property<int>("AttachmentId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("AttachmentId"), 1L, 1);

                    b.Property<int>("EmailId")
                        .HasColumnType("int");

                    b.Property<byte[]>("FileData")
                        .IsRequired()
                        .HasColumnType("varbinary(max)");

                    b.Property<string>("FileName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("AttachmentId");

                    b.HasIndex("EmailId");

                    b.ToTable("EmailAttachments");
                });

            modelBuilder.Entity("CAPSTONE_TEAM01_2024.Models.EmailRecipient", b =>
                {
                    b.Property<int>("RecipientId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("RecipientId"), 1L, 1);

                    b.Property<int>("EmailId")
                        .HasColumnType("int");

                    b.Property<string>("RecipientType")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("RecipientId");

                    b.HasIndex("EmailId");

                    b.HasIndex("UserId");

                    b.ToTable("EmailRecipients");
                });

            modelBuilder.Entity("CAPSTONE_TEAM01_2024.Models.EmailThread", b =>
                {
                    b.Property<int>("ThreadId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ThreadId"), 1L, 1);

                    b.Property<string>("Subject")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("ThreadId");

                    b.ToTable("EmailThreads");
                });

            modelBuilder.Entity("CAPSTONE_TEAM01_2024.Models.PlanDetail", b =>
                {
                    b.Property<int>("DetailId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("DetailId"), 1L, 1);

                    b.Property<int>("CriterionId")
                        .HasColumnType("int");

                    b.Property<string>("HowToExecute")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Notes")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("PlanId")
                        .HasColumnType("int");

                    b.Property<string>("Quantity")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Task")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("TimeFrame")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("DetailId");

                    b.HasIndex("CriterionId");

                    b.HasIndex("PlanId");

                    b.ToTable("PlanDetails");
                });

            modelBuilder.Entity("CAPSTONE_TEAM01_2024.Models.ReportDetail", b =>
                {
                    b.Property<int>("DetailReportlId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("DetailReportlId"), 1L, 1);

                    b.Property<int>("CriterionId")
                        .HasColumnType("int");

                    b.Property<string>("HowToExecuteReport")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("ReportId")
                        .HasColumnType("int");

                    b.Property<string>("TaskReport")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("DetailReportlId");

                    b.HasIndex("CriterionId");

                    b.HasIndex("ReportId");

                    b.ToTable("ReportDetails");
                });

            modelBuilder.Entity("CAPSTONE_TEAM01_2024.Models.SemesterPlan", b =>
                {
                    b.Property<int>("PlanId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("PlanId"), 1L, 1);

                    b.Property<string>("AdvisorName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ClassId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTime>("CreationTime")
                        .HasColumnType("datetime2");

                    b.Property<int>("PeriodId")
                        .HasColumnType("int");

                    b.Property<string>("PeriodName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PlanType")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("PlanId");

                    b.HasIndex("ClassId");

                    b.HasIndex("PeriodId");

                    b.ToTable("SemesterPlans");
                });

            modelBuilder.Entity("CAPSTONE_TEAM01_2024.Models.SemesterReport", b =>
                {
                    b.Property<int>("ReportId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ReportId"), 1L, 1);

                    b.Property<string>("AdvisorName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ClassId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTime>("CreationTimeReport")
                        .HasColumnType("datetime2");

                    b.Property<string>("FacultyAssessment")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FacultyRanking")
                        .HasColumnType("nvarchar(1)");

                    b.Property<int>("PeriodId")
                        .HasColumnType("int");

                    b.Property<string>("PeriodName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ReportType")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("SelfAssessment")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("SelfRanking")
                        .HasColumnType("nvarchar(1)");

                    b.Property<string>("StatusReport")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("ReportId");

                    b.HasIndex("ClassId");

                    b.HasIndex("PeriodId");

                    b.ToTable("SemesterReports");
                });

            modelBuilder.Entity("CAPSTONE_TEAM01_2024.Models.UserAnnouncement", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<int>("AnnouncementId")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("datetime2");

                    b.Property<bool>("IsRead")
                        .HasColumnType("bit");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("AnnouncementId");

                    b.HasIndex("UserId");

                    b.ToTable("UserAnnouncements");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasDatabaseName("RoleNameIndex")
                        .HasFilter("[NormalizedName] IS NOT NULL");

                    b.ToTable("AspNetRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<string>("ClaimType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("RoleId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUser", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("int");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Discriminator")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Email")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("bit");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("bit");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("datetimeoffset");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.Property<string>("PasswordHash")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("bit");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("bit");

                    b.Property<string>("UserName")
                        .HasMaxLength(256)
                        .HasColumnType("nvarchar(256)");

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasDatabaseName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasDatabaseName("UserNameIndex")
                        .HasFilter("[NormalizedUserName] IS NOT NULL");

                    b.ToTable("AspNetUsers", (string)null);

                    b.HasDiscriminator<string>("Discriminator").HasValue("IdentityUser");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"), 1L, 1);

                    b.Property<string>("ClaimType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ProviderKey")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("ProviderDisplayName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("RoleId")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles", (string)null);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("LoginProvider")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Value")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens", (string)null);
                });

            modelBuilder.Entity("CAPSTONE_TEAM01_2024.Models.ApplicationUser", b =>
                {
                    b.HasBaseType("Microsoft.AspNetCore.Identity.IdentityUser");

                    b.Property<string>("ClassId")
                        .IsRequired()
                        .HasColumnType("nvarchar(450)");

                    b.Property<DateTime>("DateOfBirth")
                        .HasColumnType("datetime2");

                    b.Property<string>("FullName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsRegistered")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("bit")
                        .HasDefaultValue(false);

                    b.Property<DateTime?>("LastLoginTime")
                        .HasColumnType("datetime2");

                    b.Property<string>("SchoolId")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasIndex("ClassId");

                    b.HasDiscriminator().HasValue("ApplicationUser");
                });

            modelBuilder.Entity("CAPSTONE_TEAM01_2024.Models.AttachmentReport", b =>
                {
                    b.HasOne("CAPSTONE_TEAM01_2024.Models.ReportDetail", "DetailReport")
                        .WithMany("AttachmentReport")
                        .HasForeignKey("DetailReportlId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("DetailReport");
                });

            modelBuilder.Entity("CAPSTONE_TEAM01_2024.Models.Class", b =>
                {
                    b.HasOne("CAPSTONE_TEAM01_2024.Models.ApplicationUser", "Advisor")
                        .WithMany("AdvisedClasses")
                        .HasForeignKey("AdvisorId")
                        .OnDelete(DeleteBehavior.SetNull);

                    b.Navigation("Advisor");
                });

            modelBuilder.Entity("CAPSTONE_TEAM01_2024.Models.Email", b =>
                {
                    b.HasOne("CAPSTONE_TEAM01_2024.Models.ApplicationUser", "Sender")
                        .WithMany()
                        .HasForeignKey("SenderId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("CAPSTONE_TEAM01_2024.Models.EmailThread", "Thread")
                        .WithMany("Emails")
                        .HasForeignKey("ThreadId")
                        .OnDelete(DeleteBehavior.SetNull)
                        .IsRequired();

                    b.Navigation("Sender");

                    b.Navigation("Thread");
                });

            modelBuilder.Entity("CAPSTONE_TEAM01_2024.Models.EmailAttachment", b =>
                {
                    b.HasOne("CAPSTONE_TEAM01_2024.Models.Email", "Email")
                        .WithMany("Attachments")
                        .HasForeignKey("EmailId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Email");
                });

            modelBuilder.Entity("CAPSTONE_TEAM01_2024.Models.EmailRecipient", b =>
                {
                    b.HasOne("CAPSTONE_TEAM01_2024.Models.Email", "Email")
                        .WithMany("Recipients")
                        .HasForeignKey("EmailId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("CAPSTONE_TEAM01_2024.Models.ApplicationUser", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Email");

                    b.Navigation("User");
                });

            modelBuilder.Entity("CAPSTONE_TEAM01_2024.Models.PlanDetail", b =>
                {
                    b.HasOne("CAPSTONE_TEAM01_2024.Models.Criterion", "Criterion")
                        .WithMany()
                        .HasForeignKey("CriterionId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("CAPSTONE_TEAM01_2024.Models.SemesterPlan", "SemesterPlan")
                        .WithMany("PlanDetails")
                        .HasForeignKey("PlanId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Criterion");

                    b.Navigation("SemesterPlan");
                });

            modelBuilder.Entity("CAPSTONE_TEAM01_2024.Models.ReportDetail", b =>
                {
                    b.HasOne("CAPSTONE_TEAM01_2024.Models.CriterionReport", "CriterionReport")
                        .WithMany()
                        .HasForeignKey("CriterionId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("CAPSTONE_TEAM01_2024.Models.SemesterReport", "SemesterReport")
                        .WithMany("ReportDetails")
                        .HasForeignKey("ReportId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("CriterionReport");

                    b.Navigation("SemesterReport");
                });

            modelBuilder.Entity("CAPSTONE_TEAM01_2024.Models.SemesterPlan", b =>
                {
                    b.HasOne("CAPSTONE_TEAM01_2024.Models.Class", "Class")
                        .WithMany("SemesterPlans")
                        .HasForeignKey("ClassId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("CAPSTONE_TEAM01_2024.Models.AcademicPeriod", "AcademicPeriod")
                        .WithMany("SemesterPlans")
                        .HasForeignKey("PeriodId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("AcademicPeriod");

                    b.Navigation("Class");
                });

            modelBuilder.Entity("CAPSTONE_TEAM01_2024.Models.SemesterReport", b =>
                {
                    b.HasOne("CAPSTONE_TEAM01_2024.Models.Class", "Class")
                        .WithMany("SemesterReports")
                        .HasForeignKey("ClassId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("CAPSTONE_TEAM01_2024.Models.AcademicPeriod", "AcademicPeriod")
                        .WithMany("SemesterReports")
                        .HasForeignKey("PeriodId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("AcademicPeriod");

                    b.Navigation("Class");
                });

            modelBuilder.Entity("CAPSTONE_TEAM01_2024.Models.UserAnnouncement", b =>
                {
                    b.HasOne("CAPSTONE_TEAM01_2024.Models.Announcement", "Announcement")
                        .WithMany()
                        .HasForeignKey("AnnouncementId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("CAPSTONE_TEAM01_2024.Models.ApplicationUser", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Announcement");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("CAPSTONE_TEAM01_2024.Models.ApplicationUser", b =>
                {
                    b.HasOne("CAPSTONE_TEAM01_2024.Models.Class", "EnrolledClass")
                        .WithMany("Students")
                        .HasForeignKey("ClassId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("EnrolledClass");
                });

            modelBuilder.Entity("CAPSTONE_TEAM01_2024.Models.AcademicPeriod", b =>
                {
                    b.Navigation("SemesterPlans");

                    b.Navigation("SemesterReports");
                });

            modelBuilder.Entity("CAPSTONE_TEAM01_2024.Models.Class", b =>
                {
                    b.Navigation("SemesterPlans");

                    b.Navigation("SemesterReports");

                    b.Navigation("Students");
                });

            modelBuilder.Entity("CAPSTONE_TEAM01_2024.Models.Email", b =>
                {
                    b.Navigation("Attachments");

                    b.Navigation("Recipients");
                });

            modelBuilder.Entity("CAPSTONE_TEAM01_2024.Models.EmailThread", b =>
                {
                    b.Navigation("Emails");
                });

            modelBuilder.Entity("CAPSTONE_TEAM01_2024.Models.ReportDetail", b =>
                {
                    b.Navigation("AttachmentReport");
                });

            modelBuilder.Entity("CAPSTONE_TEAM01_2024.Models.SemesterPlan", b =>
                {
                    b.Navigation("PlanDetails");
                });

            modelBuilder.Entity("CAPSTONE_TEAM01_2024.Models.SemesterReport", b =>
                {
                    b.Navigation("ReportDetails");
                });

            modelBuilder.Entity("CAPSTONE_TEAM01_2024.Models.ApplicationUser", b =>
                {
                    b.Navigation("AdvisedClasses");
                });
#pragma warning restore 612, 618
        }
    }
}
