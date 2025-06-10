using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KHRMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class remove_FK_OfRoleIds_resignationTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Resignation_EmployeeRoleMappings_RoleId",
                table: "Resignation");

            migrationBuilder.DropIndex(
                name: "IX_Resignation_RoleId",
                table: "Resignation");

            migrationBuilder.AlterColumn<string>(
                name: "RoleId",
                table: "Resignation",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "RoleId",
                table: "Resignation",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Resignation_RoleId",
                table: "Resignation",
                column: "RoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Resignation_EmployeeRoleMappings_RoleId",
                table: "Resignation",
                column: "RoleId",
                principalTable: "EmployeeRoleMappings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
