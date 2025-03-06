using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KHRMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Added_clockin_clockout_time_to_Attendance_request : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ClockInTime",
                table: "AttendanceRequests",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "ClockOutTime",
                table: "AttendanceRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ManagerId",
                table: "AttendanceRequests",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClockInTime",
                table: "AttendanceRequests");

            migrationBuilder.DropColumn(
                name: "ClockOutTime",
                table: "AttendanceRequests");

            migrationBuilder.DropColumn(
                name: "ManagerId",
                table: "AttendanceRequests");
        }
    }
}
