using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KHRMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ImproveAssetsManagementFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AssetRequests",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssetId = table.Column<long>(type: "bigint", nullable: false),
                    EmployeeId = table.Column<long>(type: "bigint", nullable: false),
                    RequestType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Priority = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1500)", maxLength: 1500, nullable: true),
                    ImageUrls = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CourierPartner = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TrackingNumber = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    DispatchedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeliveredDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReceivedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AdminRemarks = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    InspectionRemarks = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssetRequests_AssetsMasters_AssetId",
                        column: x => x.AssetId,
                        principalTable: "AssetsMasters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AssetRequests_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AssetRequestLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssetRequestId = table.Column<long>(type: "bigint", nullable: false),
                    FromStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ToStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ActionByEmployeeId = table.Column<long>(type: "bigint", nullable: true),
                    ActionByName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetRequestLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssetRequestLogs_AssetRequests_AssetRequestId",
                        column: x => x.AssetRequestId,
                        principalTable: "AssetRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssetRequestLogs_AssetRequestId",
                table: "AssetRequestLogs",
                column: "AssetRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetRequests_AssetId",
                table: "AssetRequests",
                column: "AssetId");

            migrationBuilder.CreateIndex(
                name: "IX_AssetRequests_EmployeeId",
                table: "AssetRequests",
                column: "EmployeeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssetRequestLogs");

            migrationBuilder.DropTable(
                name: "AssetRequests");
        }
    }
}
