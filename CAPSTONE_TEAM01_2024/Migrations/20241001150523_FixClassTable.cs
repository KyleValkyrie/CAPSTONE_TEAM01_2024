using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CAPSTONE_TEAM01_2024.Migrations
{
    public partial class FixClassTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Classes_AcademicPeriods_YearId",
                table: "Classes");

            migrationBuilder.DropIndex(
                name: "IX_Classes_YearId",
                table: "Classes");

            migrationBuilder.DropColumn(
                name: "YearId",
                table: "Classes");

            migrationBuilder.AlterColumn<string>(
                name: "YearName",
                table: "Classes",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "YearName",
                table: "Classes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AddColumn<int>(
                name: "YearId",
                table: "Classes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Classes_YearId",
                table: "Classes",
                column: "YearId");

            migrationBuilder.AddForeignKey(
                name: "FK_Classes_AcademicPeriods_YearId",
                table: "Classes",
                column: "YearId",
                principalTable: "AcademicPeriods",
                principalColumn: "PeriodId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
