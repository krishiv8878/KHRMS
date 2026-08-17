using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KHRMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSkillPipelineFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastUpdated",
                table: "Candidates");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Skills",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProficiencyLevel",
                table: "Skills",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "Skills");

            migrationBuilder.DropColumn(
                name: "ProficiencyLevel",
                table: "Skills");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUpdated",
                table: "Candidates",
                type: "datetime2",
                nullable: true);
        }
    }
}
