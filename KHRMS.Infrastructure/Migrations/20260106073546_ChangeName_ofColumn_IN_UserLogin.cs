using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KHRMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangeName_ofColumn_IN_UserLogin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "isPasswordRequired",
                table: "UserLogins",
                newName: "IsResetPasswordRequired");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsResetPasswordRequired",
                table: "UserLogins",
                newName: "isPasswordRequired");
        }
    }
}
