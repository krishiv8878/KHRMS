using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KHRMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Added_Regularization_Request_In_EmployeeAttendance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "TotalHours",
                table: "EmployeeAttendance",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(TimeSpan),
                oldType: "time");

            migrationBuilder.AlterColumn<DateTime>(
                name: "EffectiveHours",
                table: "EmployeeAttendance",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(TimeSpan),
                oldType: "time");

            migrationBuilder.AddColumn<bool>(
                name: "IsRegularized",
                table: "EmployeeAttendance",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "RegularizationReason",
                table: "EmployeeAttendance",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RegularizationRequestedDate",
                table: "EmployeeAttendance",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRegularized",
                table: "EmployeeAttendance");

            migrationBuilder.DropColumn(
                name: "RegularizationReason",
                table: "EmployeeAttendance");

            migrationBuilder.DropColumn(
                name: "RegularizationRequestedDate",
                table: "EmployeeAttendance");

            migrationBuilder.AlterColumn<TimeSpan>(
                name: "TotalHours",
                table: "EmployeeAttendance",
                type: "time",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<TimeSpan>(
                name: "EffectiveHours",
                table: "EmployeeAttendance",
                type: "time",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");
        }
    }
}
