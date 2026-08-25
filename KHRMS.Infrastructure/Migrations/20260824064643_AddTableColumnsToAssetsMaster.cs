using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KHRMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTableColumnsToAssetsMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AssetType",
                table: "AssetsMasters",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AssignedTo",
                table: "AssetsMasters",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "EmployeeId",
                table: "AssetsMasters",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "AssetsMasters",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "AssetsMasters",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssetsMasters_EmployeeId",
                table: "AssetsMasters",
                column: "EmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_AssetsMasters_Employees_EmployeeId",
                table: "AssetsMasters",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AssetsMasters_Employees_EmployeeId",
                table: "AssetsMasters");

            migrationBuilder.DropIndex(
                name: "IX_AssetsMasters_EmployeeId",
                table: "AssetsMasters");

            migrationBuilder.DropColumn(
                name: "AssetType",
                table: "AssetsMasters");

            migrationBuilder.DropColumn(
                name: "AssignedTo",
                table: "AssetsMasters");

            migrationBuilder.DropColumn(
                name: "EmployeeId",
                table: "AssetsMasters");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "AssetsMasters");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "AssetsMasters");
        }
    }
}
