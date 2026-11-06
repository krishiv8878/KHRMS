using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KHRMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHelpdeskTicketActivities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HelpdeskTicketActivities",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TicketId = table.Column<long>(type: "bigint", nullable: false),
                    PerformedByEmployeeId = table.Column<long>(type: "bigint", nullable: false),
                    ActivityType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OldStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    NewStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AssignedToEmployeeId = table.Column<long>(type: "bigint", nullable: true),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HelpdeskTicketActivities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HelpdeskTicketActivities_Employees_AssignedToEmployeeId",
                        column: x => x.AssignedToEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HelpdeskTicketActivities_Employees_PerformedByEmployeeId",
                        column: x => x.PerformedByEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HelpdeskTicketActivities_HelpdeskTickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "HelpdeskTickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HelpdeskTicketActivities_AssignedToEmployeeId",
                table: "HelpdeskTicketActivities",
                column: "AssignedToEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_HelpdeskTicketActivities_PerformedByEmployeeId",
                table: "HelpdeskTicketActivities",
                column: "PerformedByEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_HelpdeskTicketActivities_TicketId",
                table: "HelpdeskTicketActivities",
                column: "TicketId");

            migrationBuilder.DropForeignKey(
                name: "FK_HelpdeskTickets_Employees_AssignedToEmployeeId",
                table: "HelpdeskTickets");

            migrationBuilder.DropForeignKey(
                name: "FK_HelpdeskTickets_Employees_RequesterEmployeeId",
                table: "HelpdeskTickets");

            migrationBuilder.AddForeignKey(
                name: "FK_HelpdeskTickets_Employees_AssignedToEmployeeId",
                table: "HelpdeskTickets",
                column: "AssignedToEmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_HelpdeskTickets_Employees_RequesterEmployeeId",
                table: "HelpdeskTickets",
                column: "RequesterEmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HelpdeskTicketActivities");

            migrationBuilder.DropForeignKey(
                name: "FK_HelpdeskTickets_Employees_AssignedToEmployeeId",
                table: "HelpdeskTickets");

            migrationBuilder.DropForeignKey(
                name: "FK_HelpdeskTickets_Employees_RequesterEmployeeId",
                table: "HelpdeskTickets");

            migrationBuilder.AddForeignKey(
                name: "FK_HelpdeskTickets_Employees_AssignedToEmployeeId",
                table: "HelpdeskTickets",
                column: "AssignedToEmployeeId",
                principalTable: "Employees",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_HelpdeskTickets_Employees_RequesterEmployeeId",
                table: "HelpdeskTickets",
                column: "RequesterEmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
