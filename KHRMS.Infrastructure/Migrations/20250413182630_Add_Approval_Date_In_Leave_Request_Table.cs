using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KHRMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Add_Approval_Date_In_Leave_Request_Table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedDate",
                table: "leaveRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "leaveRequests",
                type: "nvarchar(max)",
                nullable: true);

           
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
          

            migrationBuilder.DropColumn(
                name: "ApprovedDate",
                table: "leaveRequests");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "leaveRequests");

           
        }
    }
}
