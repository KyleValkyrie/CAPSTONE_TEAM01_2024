using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CAPSTONE_TEAM01_2024.Migrations
{
    public partial class UpdateProofTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "ThumbnailData",
                table: "Proof",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0]);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ThumbnailData",
                table: "Proof");
        }
    }
}
