using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KHRMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPayrollCycleOverhaulAndLoans : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "AttendanceLockId",
                table: "PayRuns",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsLocked",
                table: "PayRuns",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "LockedByEmployeeId",
                table: "PayRuns",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LockedDate",
                table: "PayRuns",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "WorkflowInstanceId",
                table: "PayRuns",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AdjustmentReason",
                table: "PayRunEmployeeDetails",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ArrearsAmount",
                table: "PayRunEmployeeDetails",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "BonusAmount",
                table: "PayRunEmployeeDetails",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ExceptionDetails",
                table: "PayRunEmployeeDetails",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasExceptionFlag",
                table: "PayRunEmployeeDetails",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "HoldReason",
                table: "PayRunEmployeeDetails",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsOnHold",
                table: "PayRunEmployeeDetails",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "LoanEmiDeduction",
                table: "PayRunEmployeeDetails",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ManualAdjustmentAmount",
                table: "PayRunEmployeeDetails",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "EffectiveToDate",
                table: "EmployeeSalaryAssignments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCurrent",
                table: "EmployeeSalaryAssignments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "PreviousAssignmentId",
                table: "EmployeeSalaryAssignments",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "WorkflowInstanceId",
                table: "EmployeeSalaryAssignments",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EmployeeLoans",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<long>(type: "bigint", nullable: false),
                    LoanType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PrincipalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    InterestRatePercent = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    TotalRepayableAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MonthlyEmiAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TenureMonths = table.Column<int>(type: "int", nullable: false),
                    DisbursementDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RemainingBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    WorkflowInstanceId = table.Column<long>(type: "bigint", nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeLoans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeLoans_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeLoans_EmployeeId",
                table: "EmployeeLoans",
                column: "EmployeeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeeLoans");

            migrationBuilder.DropColumn(
                name: "AttendanceLockId",
                table: "PayRuns");

            migrationBuilder.DropColumn(
                name: "IsLocked",
                table: "PayRuns");

            migrationBuilder.DropColumn(
                name: "LockedByEmployeeId",
                table: "PayRuns");

            migrationBuilder.DropColumn(
                name: "LockedDate",
                table: "PayRuns");

            migrationBuilder.DropColumn(
                name: "WorkflowInstanceId",
                table: "PayRuns");

            migrationBuilder.DropColumn(
                name: "AdjustmentReason",
                table: "PayRunEmployeeDetails");

            migrationBuilder.DropColumn(
                name: "ArrearsAmount",
                table: "PayRunEmployeeDetails");

            migrationBuilder.DropColumn(
                name: "BonusAmount",
                table: "PayRunEmployeeDetails");

            migrationBuilder.DropColumn(
                name: "ExceptionDetails",
                table: "PayRunEmployeeDetails");

            migrationBuilder.DropColumn(
                name: "HasExceptionFlag",
                table: "PayRunEmployeeDetails");

            migrationBuilder.DropColumn(
                name: "HoldReason",
                table: "PayRunEmployeeDetails");

            migrationBuilder.DropColumn(
                name: "IsOnHold",
                table: "PayRunEmployeeDetails");

            migrationBuilder.DropColumn(
                name: "LoanEmiDeduction",
                table: "PayRunEmployeeDetails");

            migrationBuilder.DropColumn(
                name: "ManualAdjustmentAmount",
                table: "PayRunEmployeeDetails");

            migrationBuilder.DropColumn(
                name: "EffectiveToDate",
                table: "EmployeeSalaryAssignments");

            migrationBuilder.DropColumn(
                name: "IsCurrent",
                table: "EmployeeSalaryAssignments");

            migrationBuilder.DropColumn(
                name: "PreviousAssignmentId",
                table: "EmployeeSalaryAssignments");

            migrationBuilder.DropColumn(
                name: "WorkflowInstanceId",
                table: "EmployeeSalaryAssignments");
        }
    }
}
