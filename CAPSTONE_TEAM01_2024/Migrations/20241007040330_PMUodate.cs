using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CAPSTONE_TEAM01_2024.Migrations
{
    public partial class PMUodate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "ProfileManagers",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "ProfileManagers",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_ProfileManagers_UserId",
                table: "ProfileManagers",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProfileManagers_AspNetUsers_UserId",
                table: "ProfileManagers",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProfileManagers_AspNetUsers_UserId",
                table: "ProfileManagers");

            migrationBuilder.DropIndex(
                name: "IX_ProfileManagers_UserId",
                table: "ProfileManagers");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "ProfileManagers");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "ProfileManagers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
