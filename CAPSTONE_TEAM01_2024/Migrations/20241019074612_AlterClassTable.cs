using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CAPSTONE_TEAM01_2024.Migrations
{
    public partial class AlterClassTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "NienKhoa",
                table: "Classes",
                newName: "Term");

            migrationBuilder.RenameColumn(
                name: "Nganh",
                table: "Classes",
                newName: "Department");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Term",
                table: "Classes",
                newName: "NienKhoa");

            migrationBuilder.RenameColumn(
                name: "Department",
                table: "Classes",
                newName: "Nganh");
        }
    }
}
