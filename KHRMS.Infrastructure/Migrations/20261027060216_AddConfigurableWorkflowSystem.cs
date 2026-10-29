using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KHRMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddConfigurableWorkflowSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "WorkflowInstanceId",
                table: "Timesheets",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "WorkflowInstanceId",
                table: "ExpenseReports",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "WorkflowInstanceId",
                table: "AttendanceRequests",
                type: "bigint",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WorkflowInstanceId",
                table: "Timesheets");

            migrationBuilder.DropColumn(
                name: "WorkflowInstanceId",
                table: "ExpenseReports");

            migrationBuilder.DropColumn(
                name: "WorkflowInstanceId",
                table: "AttendanceRequests");
        }
    }
}
