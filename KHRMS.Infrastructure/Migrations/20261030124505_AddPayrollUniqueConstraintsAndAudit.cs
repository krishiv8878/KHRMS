using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KHRMS.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPayrollUniqueConstraintsAndAudit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "AccountNumber",
                table: "PayRunEmployeeDetails",
                type: "nvarchar(35)",
                maxLength: 35,
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AccountNumber",
                table: "EmployeePaymentInfo",
                type: "nvarchar(35)",
                maxLength: 35,
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.CreateIndex(
                name: "IX_Payslips_EmployeeId_Month_Year",
                table: "Payslips",
                columns: new[] { "EmployeeId", "Month", "Year" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PayRuns_Month_Year",
                table: "PayRuns",
                columns: new[] { "Month", "Year" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Payslips_EmployeeId_Month_Year",
                table: "Payslips");

            migrationBuilder.DropIndex(
                name: "IX_PayRuns_Month_Year",
                table: "PayRuns");

            migrationBuilder.AlterColumn<long>(
                name: "AccountNumber",
                table: "PayRunEmployeeDetails",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(35)",
                oldMaxLength: 35,
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "AccountNumber",
                table: "EmployeePaymentInfo",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(35)",
                oldMaxLength: 35);
        }
    }
}
