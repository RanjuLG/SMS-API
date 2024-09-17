using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SMS.Migrations
{
    /// <inheritdoc />
    public partial class manual1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_LoanPeriods_LoanPeriodId",
                table: "Transactions");

            migrationBuilder.AlterColumn<int>(
                name: "LoanPeriodId",
                table: "Transactions",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "InterestAmount",
                table: "Transactions",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LoanPeriodId",
                table: "Loans",
                type: "int",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_LoanPeriods_LoanPeriodId",
                table: "Transactions",
                column: "LoanPeriodId",
                principalTable: "LoanPeriods",
                principalColumn: "LoanPeriodId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_LoanPeriods_LoanPeriodId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "InterestAmount",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "LoanPeriodId",
                table: "Loans");

            migrationBuilder.AlterColumn<int>(
                name: "LoanPeriodId",
                table: "Transactions",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_LoanPeriods_LoanPeriodId",
                table: "Transactions",
                column: "LoanPeriodId",
                principalTable: "LoanPeriods",
                principalColumn: "LoanPeriodId");
        }
    }
}
