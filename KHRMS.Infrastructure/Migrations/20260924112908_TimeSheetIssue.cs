using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KHRMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TimeSheetIssue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Note: PermissionMaster, RolePermissionMapping, and UserPermissionMapping 
            // were already created in migration 20260922180500_Add_Permissions_And_RolePermissionMappings.
            // This migration was generated redundantly, so Up is a safe no-op.
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
