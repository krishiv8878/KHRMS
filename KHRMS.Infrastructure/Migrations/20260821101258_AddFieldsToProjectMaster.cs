using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KHRMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFieldsToProjectMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "ProjectMasters",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ProjectManagerId",
                table: "ProjectMasters",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDate",
                table: "ProjectMasters",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "ProjectMasters",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TeamSize",
                table: "ProjectMasters",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "ProjectMasters");

            migrationBuilder.DropColumn(
                name: "ProjectManagerId",
                table: "ProjectMasters");

            migrationBuilder.DropColumn(
                name: "StartDate",
                table: "ProjectMasters");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "ProjectMasters");

            migrationBuilder.DropColumn(
                name: "TeamSize",
                table: "ProjectMasters");
        }
    }
}
