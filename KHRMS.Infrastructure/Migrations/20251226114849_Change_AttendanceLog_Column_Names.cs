using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KHRMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Change_AttendanceLog_Column_Names : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AttendanceLog_Employees_employee_id",
                table: "AttendanceLog");

            migrationBuilder.RenameColumn(
                name: "duration",
                table: "AttendanceLog",
                newName: "Duration");

            migrationBuilder.RenameColumn(
                name: "out_time",
                table: "AttendanceLog",
                newName: "OutTime");

            migrationBuilder.RenameColumn(
                name: "in_time",
                table: "AttendanceLog",
                newName: "InTime");

            migrationBuilder.RenameColumn(
                name: "employee_id",
                table: "AttendanceLog",
                newName: "EmployeeId");

            migrationBuilder.RenameIndex(
                name: "IX_AttendanceLog_employee_id",
                table: "AttendanceLog",
                newName: "IX_AttendanceLog_EmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_AttendanceLog_Employees_EmployeeId",
                table: "AttendanceLog",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AttendanceLog_Employees_EmployeeId",
                table: "AttendanceLog");

            migrationBuilder.RenameColumn(
                name: "Duration",
                table: "AttendanceLog",
                newName: "duration");

            migrationBuilder.RenameColumn(
                name: "OutTime",
                table: "AttendanceLog",
                newName: "out_time");

            migrationBuilder.RenameColumn(
                name: "InTime",
                table: "AttendanceLog",
                newName: "in_time");

            migrationBuilder.RenameColumn(
                name: "EmployeeId",
                table: "AttendanceLog",
                newName: "employee_id");

            migrationBuilder.RenameIndex(
                name: "IX_AttendanceLog_EmployeeId",
                table: "AttendanceLog",
                newName: "IX_AttendanceLog_employee_id");

            migrationBuilder.AddForeignKey(
                name: "FK_AttendanceLog_Employees_employee_id",
                table: "AttendanceLog",
                column: "employee_id",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
