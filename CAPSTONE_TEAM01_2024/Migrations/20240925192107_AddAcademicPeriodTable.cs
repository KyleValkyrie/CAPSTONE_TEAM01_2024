using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CAPSTONE_TEAM01_2024.Migrations
{
    public partial class AddAcademicPeriodTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
			migrationBuilder.CreateTable(
			name: "AcademicPeriods",
			columns: table => new
			{
				PeriodId = table.Column<int>(nullable: false)
					.Annotation("SqlServer:Identity", "1, 1"),
				PeriodName = table.Column<string>(nullable: false),
				PeriodStart = table.Column<DateTime>(nullable: false),
				PeriodEnd = table.Column<DateTime>(nullable: false)
			},
			constraints: table =>
			{
				table.PrimaryKey("PK_AcademicPeriods", x => x.PeriodId);
			});
		}

        protected override void Down(MigrationBuilder migrationBuilder)
        {
			migrationBuilder.DropTable(
			name: "AcademicPeriods");
		}
    }
}
