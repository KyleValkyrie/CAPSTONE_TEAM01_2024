using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CAPSTONE_TEAM01_2024.Migrations
{
    public partial class UpdateSemesterPlanTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SemesterPlans_AcademicPeriods_AcademicPeriodPeriodId",
                table: "SemesterPlans");

            migrationBuilder.DropIndex(
                name: "IX_SemesterPlans_AcademicPeriodPeriodId",
                table: "SemesterPlans");

            migrationBuilder.DropColumn(
                name: "AcademicPeriodPeriodId",
                table: "SemesterPlans");

            migrationBuilder.CreateIndex(
                name: "IX_SemesterPlans_PeriodId",
                table: "SemesterPlans",
                column: "PeriodId");

            migrationBuilder.AddForeignKey(
                name: "FK_SemesterPlans_AcademicPeriods_PeriodId",
                table: "SemesterPlans",
                column: "PeriodId",
                principalTable: "AcademicPeriods",
                principalColumn: "PeriodId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SemesterPlans_AcademicPeriods_PeriodId",
                table: "SemesterPlans");

            migrationBuilder.DropIndex(
                name: "IX_SemesterPlans_PeriodId",
                table: "SemesterPlans");

            migrationBuilder.AddColumn<int>(
                name: "AcademicPeriodPeriodId",
                table: "SemesterPlans",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_SemesterPlans_AcademicPeriodPeriodId",
                table: "SemesterPlans",
                column: "AcademicPeriodPeriodId");

            migrationBuilder.AddForeignKey(
                name: "FK_SemesterPlans_AcademicPeriods_AcademicPeriodPeriodId",
                table: "SemesterPlans",
                column: "AcademicPeriodPeriodId",
                principalTable: "AcademicPeriods",
                principalColumn: "PeriodId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
