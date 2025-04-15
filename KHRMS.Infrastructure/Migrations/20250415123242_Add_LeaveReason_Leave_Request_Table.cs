using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KHRMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_LeaveReason_Leave_Request_Table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "leaveRequests");

           
            migrationBuilder.AddColumn<string>(
                name: "LeaveReason",
                table: "leaveRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LeaveReason",
                table: "leaveRequests");

           

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "leaveRequests",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
