using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KHRMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class changes_columntype_EmployeeAttendance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalHours",
                table: "EmployeeAttendance");
            migrationBuilder.DropColumn(
                name: "EffectiveHours",
                table: "EmployeeAttendance");
            migrationBuilder.AddColumn<decimal>(
                name: "TotalHours",
                table: "EmployeeAttendance",
                type: "decimal(18,2)",
                nullable: false);

            migrationBuilder.AddColumn<decimal>(
                name: "EffectiveHours",
                table: "EmployeeAttendance",
                type: "decimal(18,2)",
                nullable: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalHours",
                table: "EmployeeAttendance");
            migrationBuilder.DropColumn(
                name: "EffectiveHours",
                table: "EmployeeAttendance");
            migrationBuilder.AddColumn<DateTime>(
                name: "TotalHours",
                table: "EmployeeAttendance",
                type: "datetime2",
                nullable: false);
            migrationBuilder.AddColumn<DateTime>(
                name: "EffectiveHours",
                table: "EmployeeAttendance",
                type: "datetime2",
                nullable: false);
        }
    }
}
