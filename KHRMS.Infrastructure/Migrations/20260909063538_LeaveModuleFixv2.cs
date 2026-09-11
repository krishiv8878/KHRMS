using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KHRMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class LeaveModuleFixv2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApprovedBy",
                table: "leaveRequests");

            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "leaveRequests");

            migrationBuilder.RenameColumn(
                name: "ApprovedDate",
                table: "leaveRequests",
                newName: "ActionDate");

            migrationBuilder.AddColumn<long>(
                name: "ActionBy",
                table: "leaveRequests",
                type: "bigint",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ActionBy",
                table: "leaveRequests");

            migrationBuilder.RenameColumn(
                name: "ActionDate",
                table: "leaveRequests",
                newName: "ApprovedDate");

            migrationBuilder.AddColumn<int>(
                name: "ApprovedBy",
                table: "leaveRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "leaveRequests",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
