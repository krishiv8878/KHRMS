using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KHRMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddChecklistsNoticePolicyAndSystemSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedLwd",
                table: "Resignation",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AssetClearanceOverrideReason",
                table: "Resignation",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExitInterviewRemarks",
                table: "Resignation",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FnFPreparedDate",
                table: "Resignation",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FnFRemarks",
                table: "Resignation",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FnFStatus",
                table: "Resignation",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HrRemarks",
                table: "Resignation",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsLwdLocked",
                table: "Resignation",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ManagerRemarks",
                table: "Resignation",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NoticeHandling",
                table: "Resignation",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NoticePeriodDaysSnapshot",
                table: "Resignation",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NoticePeriodSourceSnapshot",
                table: "Resignation",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NoticeShortfallDays",
                table: "Resignation",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ProposedLwd",
                table: "Resignation",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RequestedLwd",
                table: "Resignation",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResignationStatus",
                table: "Resignation",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmploymentStatus",
                table: "Employees",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmploymentType",
                table: "Employees",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExitDate",
                table: "Employees",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NoticePeriodOverrideDays",
                table: "Employees",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NoticePeriodOverrideReason",
                table: "Employees",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PanNumber",
                table: "Employees",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PersonId",
                table: "Employees",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PersonalEmail",
                table: "Employees",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "PreviousEmployeeId",
                table: "Employees",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ProbationEndDate",
                table: "Employees",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProbationStatus",
                table: "Employees",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RehireDate",
                table: "Employees",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UanNumber",
                table: "Employees",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NoticePeriodDays",
                table: "Designations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ProbationNoticePeriodDays",
                table: "Designations",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ChecklistTemplates",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ChecklistType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Department = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChecklistTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SystemSettings",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SettingKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SettingValue = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChecklistTemplateTasks",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TemplateId = table.Column<long>(type: "bigint", nullable: false),
                    TaskName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DefaultAssigneeRole = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DefaultAssigneeEmployeeId = table.Column<long>(type: "bigint", nullable: true),
                    DueDays = table.Column<int>(type: "int", nullable: false),
                    IsMandatory = table.Column<bool>(type: "bit", nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChecklistTemplateTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChecklistTemplateTasks_ChecklistTemplates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "ChecklistTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeChecklists",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<long>(type: "bigint", nullable: false),
                    TemplateId = table.Column<long>(type: "bigint", nullable: true),
                    ChecklistType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TotalTasks = table.Column<int>(type: "int", nullable: false),
                    CompletedTasks = table.Column<int>(type: "int", nullable: false),
                    InitiatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeChecklists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeChecklists_ChecklistTemplates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "ChecklistTemplates",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EmployeeChecklists_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ChecklistTasks",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeChecklistId = table.Column<long>(type: "bigint", nullable: false),
                    TaskName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AssignedEmployeeId = table.Column<long>(type: "bigint", nullable: true),
                    AssignedRole = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsMandatory = table.Column<bool>(type: "bit", nullable: false),
                    ClearedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ClearedByEmployeeId = table.Column<long>(type: "bigint", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    WaiveReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    WaivedByEmployeeId = table.Column<long>(type: "bigint", nullable: true),
                    AttachmentUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChecklistTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChecklistTasks_EmployeeChecklists_EmployeeChecklistId",
                        column: x => x.EmployeeChecklistId,
                        principalTable: "EmployeeChecklists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChecklistTasks_Employees_AssignedEmployeeId",
                        column: x => x.AssignedEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ChecklistTasks_Employees_ClearedByEmployeeId",
                        column: x => x.ClearedByEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ChecklistTasks_Employees_WaivedByEmployeeId",
                        column: x => x.WaivedByEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id");
                });

            migrationBuilder.InsertData(
                table: "SystemSettings",
                columns: new[] { "Id", "CreatedBy", "CreatedDate", "Description", "IsActive", "IsDeleted", "SettingKey", "SettingValue", "UpdatedBy", "UpdatedDate" },
                values: new object[] { 1L, 0, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Determines whether rehired employees retain their previous employee code or receive a new one.", true, false, "ReusePreviousEmployeeCodeOnRehire", "false", 0, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.CreateIndex(
                name: "IX_Employees_PanNumber_UanNumber_PersonalEmail",
                table: "Employees",
                columns: new[] { "PanNumber", "UanNumber", "PersonalEmail" });

            migrationBuilder.CreateIndex(
                name: "IX_Employees_PreviousEmployeeId",
                table: "Employees",
                column: "PreviousEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_ChecklistTasks_AssignedEmployeeId",
                table: "ChecklistTasks",
                column: "AssignedEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_ChecklistTasks_ClearedByEmployeeId",
                table: "ChecklistTasks",
                column: "ClearedByEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_ChecklistTasks_EmployeeChecklistId",
                table: "ChecklistTasks",
                column: "EmployeeChecklistId");

            migrationBuilder.CreateIndex(
                name: "IX_ChecklistTasks_WaivedByEmployeeId",
                table: "ChecklistTasks",
                column: "WaivedByEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_ChecklistTemplateTasks_TemplateId",
                table: "ChecklistTemplateTasks",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeChecklists_EmployeeId",
                table: "EmployeeChecklists",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeChecklists_TemplateId",
                table: "EmployeeChecklists",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_SystemSettings_SettingKey",
                table: "SystemSettings",
                column: "SettingKey",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Employees_PreviousEmployeeId",
                table: "Employees",
                column: "PreviousEmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Employees_PreviousEmployeeId",
                table: "Employees");

            migrationBuilder.DropTable(
                name: "ChecklistTasks");

            migrationBuilder.DropTable(
                name: "ChecklistTemplateTasks");

            migrationBuilder.DropTable(
                name: "SystemSettings");

            migrationBuilder.DropTable(
                name: "EmployeeChecklists");

            migrationBuilder.DropTable(
                name: "ChecklistTemplates");

            migrationBuilder.DropIndex(
                name: "IX_Employees_PanNumber_UanNumber_PersonalEmail",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Employees_PreviousEmployeeId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "ApprovedLwd",
                table: "Resignation");

            migrationBuilder.DropColumn(
                name: "AssetClearanceOverrideReason",
                table: "Resignation");

            migrationBuilder.DropColumn(
                name: "ExitInterviewRemarks",
                table: "Resignation");

            migrationBuilder.DropColumn(
                name: "FnFPreparedDate",
                table: "Resignation");

            migrationBuilder.DropColumn(
                name: "FnFRemarks",
                table: "Resignation");

            migrationBuilder.DropColumn(
                name: "FnFStatus",
                table: "Resignation");

            migrationBuilder.DropColumn(
                name: "HrRemarks",
                table: "Resignation");

            migrationBuilder.DropColumn(
                name: "IsLwdLocked",
                table: "Resignation");

            migrationBuilder.DropColumn(
                name: "ManagerRemarks",
                table: "Resignation");

            migrationBuilder.DropColumn(
                name: "NoticeHandling",
                table: "Resignation");

            migrationBuilder.DropColumn(
                name: "NoticePeriodDaysSnapshot",
                table: "Resignation");

            migrationBuilder.DropColumn(
                name: "NoticePeriodSourceSnapshot",
                table: "Resignation");

            migrationBuilder.DropColumn(
                name: "NoticeShortfallDays",
                table: "Resignation");

            migrationBuilder.DropColumn(
                name: "ProposedLwd",
                table: "Resignation");

            migrationBuilder.DropColumn(
                name: "RequestedLwd",
                table: "Resignation");

            migrationBuilder.DropColumn(
                name: "ResignationStatus",
                table: "Resignation");

            migrationBuilder.DropColumn(
                name: "EmploymentStatus",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "EmploymentType",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "ExitDate",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "NoticePeriodOverrideDays",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "NoticePeriodOverrideReason",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "PanNumber",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "PersonId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "PersonalEmail",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "PreviousEmployeeId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "ProbationEndDate",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "ProbationStatus",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "RehireDate",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "UanNumber",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "NoticePeriodDays",
                table: "Designations");

            migrationBuilder.DropColumn(
                name: "ProbationNoticePeriodDays",
                table: "Designations");
        }
    }
}
