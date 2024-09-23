using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SMS.Migrations
{
    /// <inheritdoc />
    public partial class loanPerioedremovedfromtransaction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_LoanPeriods_LoanPeriodId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_LoanPeriodId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "LoanPeriodId",
                table: "Transactions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LoanPeriodId",
                table: "Transactions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_LoanPeriodId",
                table: "Transactions",
                column: "LoanPeriodId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_LoanPeriods_LoanPeriodId",
                table: "Transactions",
                column: "LoanPeriodId",
                principalTable: "LoanPeriods",
                principalColumn: "LoanPeriodId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
