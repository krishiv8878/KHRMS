using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KHRMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLeavePolicyEngineAndLedger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AccrualAmount",
                table: "LeaveType",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "AccrualEnabled",
                table: "LeaveType",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "AccrualFrequency",
                table: "LeaveType",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ApplicableEmploymentType",
                table: "LeaveType",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ApplicableGender",
                table: "LeaveType",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "CarryForwardEnabled",
                table: "LeaveType",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "CompOffValidityDays",
                table: "LeaveType",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "EncashmentAllowed",
                table: "LeaveType",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "EncashmentSalaryFormula",
                table: "LeaveType",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ExcessBalanceTreatment",
                table: "LeaveType",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsCompOff",
                table: "LeaveType",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsNegativeBalanceAllowed",
                table: "LeaveType",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPaid",
                table: "LeaveType",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "MaxCarryForwardDays",
                table: "LeaveType",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "MaxConsecutiveDays",
                table: "LeaveType",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "MaxEncashmentDaysPerRequest",
                table: "LeaveType",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "MaxSandwichLeavesAllowed",
                table: "LeaveType",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "MinBalanceForEncashment",
                table: "LeaveType",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "NegativeBalanceLimit",
                table: "LeaveType",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "NoticeDaysRequired",
                table: "LeaveType",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "SandwichRuleEnabled",
                table: "LeaveType",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsConvertedFromLop",
                table: "leaveRequests",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsLop",
                table: "leaveRequests",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSandwichApplied",
                table: "leaveRequests",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "LopDays",
                table: "leaveRequests",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<long>(
                name: "OriginalLopRequestId",
                table: "leaveRequests",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PaidDays",
                table: "leaveRequests",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SandwichDays",
                table: "leaveRequests",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalWorkingDays",
                table: "leaveRequests",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "CompOffGrantRequests",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<long>(type: "bigint", nullable: false),
                    AttendanceDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    WorkedMinutes = table.Column<int>(type: "int", nullable: false),
                    EligibleDays = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ActionBy = table.Column<long>(type: "bigint", nullable: true),
                    ActionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    WorkflowInstanceId = table.Column<long>(type: "bigint", nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompOffGrantRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompOffGrantRequests_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LeaveEncashmentRequests",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<long>(type: "bigint", nullable: false),
                    LeaveTypeId = table.Column<long>(type: "bigint", nullable: false),
                    RequestedDays = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DailySalaryBasis = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EstimatedAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ActionBy = table.Column<long>(type: "bigint", nullable: true),
                    ActionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PayRunId = table.Column<long>(type: "bigint", nullable: true),
                    WorkflowInstanceId = table.Column<long>(type: "bigint", nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaveEncashmentRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeaveEncashmentRequests_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LeaveEncashmentRequests_LeaveType_LeaveTypeId",
                        column: x => x.LeaveTypeId,
                        principalTable: "LeaveType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LeaveEntitlements",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<long>(type: "bigint", nullable: false),
                    LeaveTypeId = table.Column<long>(type: "bigint", nullable: false),
                    SourceType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SourceReferenceId = table.Column<long>(type: "bigint", nullable: true),
                    TotalDays = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ConsumedDays = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RemainingDays = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreditDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsExpired = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaveEntitlements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeaveEntitlements_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LeaveEntitlements_LeaveType_LeaveTypeId",
                        column: x => x.LeaveTypeId,
                        principalTable: "LeaveType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LeaveLedgers",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<long>(type: "bigint", nullable: false),
                    LeaveTypeId = table.Column<long>(type: "bigint", nullable: false),
                    TransactionType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Days = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BalanceAfter = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LeaveRequestId = table.Column<long>(type: "bigint", nullable: true),
                    EffectiveDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Period = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaveLedgers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeaveLedgers_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LeaveLedgers_LeaveType_LeaveTypeId",
                        column: x => x.LeaveTypeId,
                        principalTable: "LeaveType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LeaveLedgers_leaveRequests_LeaveRequestId",
                        column: x => x.LeaveRequestId,
                        principalTable: "leaveRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "LopConversionRequests",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<long>(type: "bigint", nullable: false),
                    OriginalLeaveRequestId = table.Column<long>(type: "bigint", nullable: false),
                    TargetLeaveTypeId = table.Column<long>(type: "bigint", nullable: false),
                    ConversionDays = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FromDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ToDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ActionBy = table.Column<long>(type: "bigint", nullable: true),
                    ActionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    WorkflowInstanceId = table.Column<long>(type: "bigint", nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LopConversionRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LopConversionRequests_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LopConversionRequests_LeaveType_TargetLeaveTypeId",
                        column: x => x.TargetLeaveTypeId,
                        principalTable: "LeaveType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LopConversionRequests_leaveRequests_OriginalLeaveRequestId",
                        column: x => x.OriginalLeaveRequestId,
                        principalTable: "leaveRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CompOffGrantRequests_EmployeeId",
                table: "CompOffGrantRequests",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveEncashmentRequests_EmployeeId",
                table: "LeaveEncashmentRequests",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveEncashmentRequests_LeaveTypeId",
                table: "LeaveEncashmentRequests",
                column: "LeaveTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveEntitlements_EmployeeId_LeaveTypeId_IsExpired",
                table: "LeaveEntitlements",
                columns: new[] { "EmployeeId", "LeaveTypeId", "IsExpired" });

            migrationBuilder.CreateIndex(
                name: "IX_LeaveEntitlements_LeaveTypeId",
                table: "LeaveEntitlements",
                column: "LeaveTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveLedgers_EmployeeId_LeaveTypeId",
                table: "LeaveLedgers",
                columns: new[] { "EmployeeId", "LeaveTypeId" });

            migrationBuilder.CreateIndex(
                name: "IX_LeaveLedgers_EmployeeId_Period",
                table: "LeaveLedgers",
                columns: new[] { "EmployeeId", "Period" });

            migrationBuilder.CreateIndex(
                name: "IX_LeaveLedgers_LeaveRequestId",
                table: "LeaveLedgers",
                column: "LeaveRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaveLedgers_LeaveTypeId",
                table: "LeaveLedgers",
                column: "LeaveTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_LopConversionRequests_EmployeeId",
                table: "LopConversionRequests",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_LopConversionRequests_OriginalLeaveRequestId",
                table: "LopConversionRequests",
                column: "OriginalLeaveRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_LopConversionRequests_TargetLeaveTypeId",
                table: "LopConversionRequests",
                column: "TargetLeaveTypeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompOffGrantRequests");

            migrationBuilder.DropTable(
                name: "LeaveEncashmentRequests");

            migrationBuilder.DropTable(
                name: "LeaveEntitlements");

            migrationBuilder.DropTable(
                name: "LeaveLedgers");

            migrationBuilder.DropTable(
                name: "LopConversionRequests");

            migrationBuilder.DropColumn(
                name: "AccrualAmount",
                table: "LeaveType");

            migrationBuilder.DropColumn(
                name: "AccrualEnabled",
                table: "LeaveType");

            migrationBuilder.DropColumn(
                name: "AccrualFrequency",
                table: "LeaveType");

            migrationBuilder.DropColumn(
                name: "ApplicableEmploymentType",
                table: "LeaveType");

            migrationBuilder.DropColumn(
                name: "ApplicableGender",
                table: "LeaveType");

            migrationBuilder.DropColumn(
                name: "CarryForwardEnabled",
                table: "LeaveType");

            migrationBuilder.DropColumn(
                name: "CompOffValidityDays",
                table: "LeaveType");

            migrationBuilder.DropColumn(
                name: "EncashmentAllowed",
                table: "LeaveType");

            migrationBuilder.DropColumn(
                name: "EncashmentSalaryFormula",
                table: "LeaveType");

            migrationBuilder.DropColumn(
                name: "ExcessBalanceTreatment",
                table: "LeaveType");

            migrationBuilder.DropColumn(
                name: "IsCompOff",
                table: "LeaveType");

            migrationBuilder.DropColumn(
                name: "IsNegativeBalanceAllowed",
                table: "LeaveType");

            migrationBuilder.DropColumn(
                name: "IsPaid",
                table: "LeaveType");

            migrationBuilder.DropColumn(
                name: "MaxCarryForwardDays",
                table: "LeaveType");

            migrationBuilder.DropColumn(
                name: "MaxConsecutiveDays",
                table: "LeaveType");

            migrationBuilder.DropColumn(
                name: "MaxEncashmentDaysPerRequest",
                table: "LeaveType");

            migrationBuilder.DropColumn(
                name: "MaxSandwichLeavesAllowed",
                table: "LeaveType");

            migrationBuilder.DropColumn(
                name: "MinBalanceForEncashment",
                table: "LeaveType");

            migrationBuilder.DropColumn(
                name: "NegativeBalanceLimit",
                table: "LeaveType");

            migrationBuilder.DropColumn(
                name: "NoticeDaysRequired",
                table: "LeaveType");

            migrationBuilder.DropColumn(
                name: "SandwichRuleEnabled",
                table: "LeaveType");

            migrationBuilder.DropColumn(
                name: "IsConvertedFromLop",
                table: "leaveRequests");

            migrationBuilder.DropColumn(
                name: "IsLop",
                table: "leaveRequests");

            migrationBuilder.DropColumn(
                name: "IsSandwichApplied",
                table: "leaveRequests");

            migrationBuilder.DropColumn(
                name: "LopDays",
                table: "leaveRequests");

            migrationBuilder.DropColumn(
                name: "OriginalLopRequestId",
                table: "leaveRequests");

            migrationBuilder.DropColumn(
                name: "PaidDays",
                table: "leaveRequests");

            migrationBuilder.DropColumn(
                name: "SandwichDays",
                table: "leaveRequests");

            migrationBuilder.DropColumn(
                name: "TotalWorkingDays",
                table: "leaveRequests");
        }
    }
}
