using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KHRMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FK_in_AttendanceRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AttendanceRequests_Employees_EmployeeId",
                table: "AttendanceRequests");

            migrationBuilder.DropIndex(
                name: "IX_AttendanceRequests_EmployeeId",
                table: "AttendanceRequests");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Resignation_Date",
                table: "Resignation",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NoticePeriod",
                table: "Resignation",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsApproved",
                table: "leaveRequests",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceRequests_ManagerId",
                table: "AttendanceRequests",
                column: "ManagerId");

            migrationBuilder.AddForeignKey(
                name: "FK_AttendanceRequests_Employees_ManagerId",
                table: "AttendanceRequests",
                column: "ManagerId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AttendanceRequests_Employees_ManagerId",
                table: "AttendanceRequests");

            migrationBuilder.DropIndex(
                name: "IX_AttendanceRequests_ManagerId",
                table: "AttendanceRequests");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Resignation_Date",
                table: "Resignation",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<string>(
                name: "NoticePeriod",
                table: "Resignation",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<bool>(
                name: "IsApproved",
                table: "leaveRequests",
                type: "bit",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceRequests_EmployeeId",
                table: "AttendanceRequests",
                column: "EmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_AttendanceRequests_Employees_EmployeeId",
                table: "AttendanceRequests",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
