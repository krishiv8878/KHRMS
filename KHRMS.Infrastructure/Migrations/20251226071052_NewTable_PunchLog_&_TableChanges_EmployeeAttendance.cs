using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KHRMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class NewTable_PunchLog__TableChanges_EmployeeAttendance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.DropColumn(
                name: "RegularizedBy",
                table: "EmployeeAttendance");

            migrationBuilder.DropColumn(
                name: "RegularizedDate",
                table: "EmployeeAttendance");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ClockOut",
                table: "EmployeeAttendance",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<DateOnly>(
                name: "AttendanceDate",
                table: "EmployeeAttendance",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.CreateTable(
                name: "PunchLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    employee_id = table.Column<long>(type: "bigint", nullable: false),
                    AttendanceDate = table.Column<DateOnly>(type: "date", nullable: false),
                    in_time = table.Column<DateTime>(type: "datetime2", nullable: true),
                    out_time = table.Column<DateTime>(type: "datetime2", nullable: true),
                    duration = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PunchLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PunchLogs_Employees_employee_id",
                        column: x => x.employee_id,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PunchLogs_employee_id",
                table: "PunchLogs",
                column: "employee_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PunchLogs");

            migrationBuilder.DropColumn(
                name: "AttendanceDate",
                table: "EmployeeAttendance");

            migrationBuilder.AlterColumn<DateTime>(
                name: "ClockOut",
                table: "EmployeeAttendance",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsRegularized",
                table: "EmployeeAttendance",
                type: "bit",
                nullable: true);

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

            migrationBuilder.AddColumn<long>(
                name: "RegularizedBy",
                table: "EmployeeAttendance",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RegularizedDate",
                table: "EmployeeAttendance",
                type: "datetime2",
                nullable: true);
        }
    }
}
