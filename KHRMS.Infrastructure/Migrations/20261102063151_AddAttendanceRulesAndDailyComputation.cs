using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KHRMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAttendanceRulesAndDailyComputation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AutoClockOutBufferHours",
                table: "ShiftMasters",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "BreakEndTime",
                table: "ShiftMasters",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BreakStartTime",
                table: "ShiftMasters",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FullDayMinimumMinutes",
                table: "ShiftMasters",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GracePeriodMinutes",
                table: "ShiftMasters",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "HalfDayMinimumMinutes",
                table: "ShiftMasters",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "LateMarkPenalty",
                table: "ShiftMasters",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "MaxLateMarksPerMonth",
                table: "ShiftMasters",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PresentMinimumMinutes",
                table: "ShiftMasters",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ScheduledBreakMinutes",
                table: "ShiftMasters",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "AttendanceLocks",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Month = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    TotalEmployeesLocked = table.Column<int>(type: "int", nullable: false),
                    TotalPresentDays = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalLopDays = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalOvertimeHours = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsLocked = table.Column<bool>(type: "bit", nullable: false),
                    LockedByEmployeeId = table.Column<long>(type: "bigint", nullable: false),
                    LockedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UnlockedReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UnlockedByEmployeeId = table.Column<long>(type: "bigint", nullable: true),
                    UnlockedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttendanceLocks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DailyAttendanceRecords",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<long>(type: "bigint", nullable: false),
                    AttendanceDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ShiftId = table.Column<long>(type: "bigint", nullable: true),
                    FirstClockIn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastClockOut = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TotalWorkMinutes = table.Column<int>(type: "int", nullable: false),
                    TotalBreakMinutes = table.Column<int>(type: "int", nullable: false),
                    EffectiveWorkMinutes = table.Column<int>(type: "int", nullable: false),
                    OvertimeMinutes = table.Column<int>(type: "int", nullable: false),
                    LateArrivalMinutes = table.Column<int>(type: "int", nullable: false),
                    EarlyExitMinutes = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    LeaveRequestId = table.Column<long>(type: "bigint", nullable: true),
                    IsRegularized = table.Column<bool>(type: "bit", nullable: false),
                    AttendanceLockId = table.Column<long>(type: "bigint", nullable: true),
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
                    table.PrimaryKey("PK_DailyAttendanceRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DailyAttendanceRecords_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DailyAttendanceRecords_ShiftMasters_ShiftId",
                        column: x => x.ShiftId,
                        principalTable: "ShiftMasters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeShiftAssignments",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<long>(type: "bigint", nullable: false),
                    ShiftId = table.Column<long>(type: "bigint", nullable: false),
                    EffectiveFromDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EffectiveToDate = table.Column<DateOnly>(type: "date", nullable: true),
                    WeekOffPattern = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeShiftAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeShiftAssignments_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmployeeShiftAssignments_ShiftMasters_ShiftId",
                        column: x => x.ShiftId,
                        principalTable: "ShiftMasters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceLocks_Month_Year",
                table: "AttendanceLocks",
                columns: new[] { "Month", "Year" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DailyAttendanceRecords_AttendanceDate_Status",
                table: "DailyAttendanceRecords",
                columns: new[] { "AttendanceDate", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_DailyAttendanceRecords_EmployeeId_AttendanceDate",
                table: "DailyAttendanceRecords",
                columns: new[] { "EmployeeId", "AttendanceDate" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DailyAttendanceRecords_ShiftId",
                table: "DailyAttendanceRecords",
                column: "ShiftId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeShiftAssignments_EmployeeId_EffectiveFromDate",
                table: "EmployeeShiftAssignments",
                columns: new[] { "EmployeeId", "EffectiveFromDate" });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeShiftAssignments_ShiftId",
                table: "EmployeeShiftAssignments",
                column: "ShiftId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AttendanceLocks");

            migrationBuilder.DropTable(
                name: "DailyAttendanceRecords");

            migrationBuilder.DropTable(
                name: "EmployeeShiftAssignments");

            migrationBuilder.DropColumn(
                name: "AutoClockOutBufferHours",
                table: "ShiftMasters");

            migrationBuilder.DropColumn(
                name: "BreakEndTime",
                table: "ShiftMasters");

            migrationBuilder.DropColumn(
                name: "BreakStartTime",
                table: "ShiftMasters");

            migrationBuilder.DropColumn(
                name: "FullDayMinimumMinutes",
                table: "ShiftMasters");

            migrationBuilder.DropColumn(
                name: "GracePeriodMinutes",
                table: "ShiftMasters");

            migrationBuilder.DropColumn(
                name: "HalfDayMinimumMinutes",
                table: "ShiftMasters");

            migrationBuilder.DropColumn(
                name: "LateMarkPenalty",
                table: "ShiftMasters");

            migrationBuilder.DropColumn(
                name: "MaxLateMarksPerMonth",
                table: "ShiftMasters");

            migrationBuilder.DropColumn(
                name: "PresentMinimumMinutes",
                table: "ShiftMasters");

            migrationBuilder.DropColumn(
                name: "ScheduledBreakMinutes",
                table: "ShiftMasters");
        }
    }
}
