using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CAPSTONE_TEAM01_2024.Migrations
{
    public partial class AddReportDetailsToCriterionReport : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CriterionReports",
                columns: table => new
                {
                    CriterionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NameReport = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DescriptionReport = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CriterionReports", x => x.CriterionId);
                });

            migrationBuilder.CreateTable(
                name: "SemesterReports",
                columns: table => new
                {
                    ReportId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PeriodName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReportType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ClassId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AdvisorName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreationTimeReport = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StatusReport = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PeriodId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SemesterReports", x => x.ReportId);
                    table.ForeignKey(
                        name: "FK_SemesterReports_AcademicPeriods_PeriodId",
                        column: x => x.PeriodId,
                        principalTable: "AcademicPeriods",
                        principalColumn: "PeriodId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SemesterReports_Classes_ClassId",
                        column: x => x.ClassId,
                        principalTable: "Classes",
                        principalColumn: "ClassId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReportDetails",
                columns: table => new
                {
                    DetailReportlId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReportId = table.Column<int>(type: "int", nullable: false),
                    CriterionId = table.Column<int>(type: "int", nullable: false),
                    TaskReport = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HowToExecuteReport = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SelfAssessment = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SelfRanking = table.Column<int>(type: "int", nullable: false),
                    FacultyAssessment = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FacultyRanking = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReportDetails", x => x.DetailReportlId);
                    table.ForeignKey(
                        name: "FK_ReportDetails_CriterionReports_CriterionId",
                        column: x => x.CriterionId,
                        principalTable: "CriterionReports",
                        principalColumn: "CriterionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReportDetails_SemesterReports_ReportId",
                        column: x => x.ReportId,
                        principalTable: "SemesterReports",
                        principalColumn: "ReportId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AttachmentReports",
                columns: table => new
                {
                    AttachmentReportId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileNames = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileDatas = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    DetailReportlId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttachmentReports", x => x.AttachmentReportId);
                    table.ForeignKey(
                        name: "FK_AttachmentReports_ReportDetails_DetailReportlId",
                        column: x => x.DetailReportlId,
                        principalTable: "ReportDetails",
                        principalColumn: "DetailReportlId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AttachmentReports_DetailReportlId",
                table: "AttachmentReports",
                column: "DetailReportlId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportDetails_CriterionId",
                table: "ReportDetails",
                column: "CriterionId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportDetails_ReportId",
                table: "ReportDetails",
                column: "ReportId");

            migrationBuilder.CreateIndex(
                name: "IX_SemesterReports_ClassId",
                table: "SemesterReports",
                column: "ClassId");

            migrationBuilder.CreateIndex(
                name: "IX_SemesterReports_PeriodId",
                table: "SemesterReports",
                column: "PeriodId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AttachmentReports");

            migrationBuilder.DropTable(
                name: "ReportDetails");

            migrationBuilder.DropTable(
                name: "CriterionReports");

            migrationBuilder.DropTable(
                name: "SemesterReports");
        }
    }
}
