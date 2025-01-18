using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CAPSTONE_TEAM01_2024.Migrations
{
    public partial class EmailNotificationModifications : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRead",
                table: "Emails");

            migrationBuilder.AddColumn<bool>(
                name: "IsRead",
                table: "EmailRecipients",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRead",
                table: "EmailRecipients");

            migrationBuilder.AddColumn<bool>(
                name: "IsRead",
                table: "Emails",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
