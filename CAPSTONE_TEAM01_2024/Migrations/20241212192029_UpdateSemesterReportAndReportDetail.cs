using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CAPSTONE_TEAM01_2024.Migrations
{
    public partial class UpdateSemesterReportAndReportDetail : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FacultyAssessment",
                table: "ReportDetails");

            migrationBuilder.DropColumn(
                name: "FacultyRanking",
                table: "ReportDetails");

            migrationBuilder.DropColumn(
                name: "SelfAssessment",
                table: "ReportDetails");

            migrationBuilder.DropColumn(
                name: "SelfRanking",
                table: "ReportDetails");

            migrationBuilder.AddColumn<string>(
                name: "FacultyAssessment",
                table: "SemesterReports",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FacultyRanking",
                table: "SemesterReports",
                type: "nvarchar(1)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SelfAssessment",
                table: "SemesterReports",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SelfRanking",
                table: "SemesterReports",
                type: "nvarchar(1)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FacultyAssessment",
                table: "SemesterReports");

            migrationBuilder.DropColumn(
                name: "FacultyRanking",
                table: "SemesterReports");

            migrationBuilder.DropColumn(
                name: "SelfAssessment",
                table: "SemesterReports");

            migrationBuilder.DropColumn(
                name: "SelfRanking",
                table: "SemesterReports");

            migrationBuilder.AddColumn<string>(
                name: "FacultyAssessment",
                table: "ReportDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "FacultyRanking",
                table: "ReportDetails",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SelfAssessment",
                table: "ReportDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "SelfRanking",
                table: "ReportDetails",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
