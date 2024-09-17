using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SMS.Migrations
{
    /// <inheritdoc />
    public partial class loanPeriodUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Loans_LoanPeriodId",
                table: "Loans",
                column: "LoanPeriodId");

            migrationBuilder.AddForeignKey(
                name: "FK_Loans_LoanPeriods_LoanPeriodId",
                table: "Loans",
                column: "LoanPeriodId",
                principalTable: "LoanPeriods",
                principalColumn: "LoanPeriodId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Loans_LoanPeriods_LoanPeriodId",
                table: "Loans");

            migrationBuilder.DropIndex(
                name: "IX_Loans_LoanPeriodId",
                table: "Loans");
        }
    }
}
