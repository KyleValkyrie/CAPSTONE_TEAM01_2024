using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CAPSTONE_TEAM01_2024.Migrations
{
    public partial class AllowMultipleFilesUpload : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProofFile",
                table: "SemesterPlans");

            migrationBuilder.CreateTable(
                name: "Proof",
                columns: table => new
                {
                    ProofId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FileData = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SemesterPlanId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Proof", x => x.ProofId);
                    table.ForeignKey(
                        name: "FK_Proof_SemesterPlans_SemesterPlanId",
                        column: x => x.SemesterPlanId,
                        principalTable: "SemesterPlans",
                        principalColumn: "PlanId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Proof_SemesterPlanId",
                table: "Proof",
                column: "SemesterPlanId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Proof");

            migrationBuilder.AddColumn<byte[]>(
                name: "ProofFile",
                table: "SemesterPlans",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0]);
        }
    }
}
