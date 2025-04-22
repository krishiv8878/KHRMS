using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KHRMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Added_Regularization_ApproveedBy_Request_In_EmployeeAttendance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RegularizedBy",
                table: "EmployeeAttendance");

            migrationBuilder.DropColumn(
                name: "RegularizedDate",
                table: "EmployeeAttendance");
        }
    }
}
