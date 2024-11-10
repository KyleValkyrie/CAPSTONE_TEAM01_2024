using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CAPSTONE_TEAM01_2024.Migrations
{
    public partial class ModifyAttachmentTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "EmailAttachments");

            migrationBuilder.AddColumn<byte[]>(
                name: "FileData",
                table: "EmailAttachments",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0]);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileData",
                table: "EmailAttachments");

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "EmailAttachments",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
