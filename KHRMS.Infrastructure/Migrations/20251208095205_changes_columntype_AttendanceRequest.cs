using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KHRMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class changes_columntype_AttendanceRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AttendanceRequests_Employees_ManagerId",
                table: "AttendanceRequests");

            migrationBuilder.DropIndex(
                name: "IX_AttendanceRequests_ManagerId",
                table: "AttendanceRequests");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AttendanceRequests_Employees_EmployeeId",
                table: "AttendanceRequests");

            migrationBuilder.DropIndex(
                name: "IX_AttendanceRequests_EmployeeId",
                table: "AttendanceRequests");

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
    }
}
