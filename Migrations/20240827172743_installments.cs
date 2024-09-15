using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SMS.Migrations
{
    /// <inheritdoc />
    public partial class installments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LoanPeriodId",
                table: "Transactions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Installments",
                columns: table => new
                {
                    InstallmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TransactionId = table.Column<int>(type: "int", nullable: false),
                    AmountDue = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AmountPaid = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Installments", x => x.InstallmentId);
                    table.ForeignKey(
                        name: "FK_Installments_Transactions_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "Transactions",
                        principalColumn: "TransactionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_LoanPeriodId",
                table: "Transactions",
                column: "LoanPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_Installments_TransactionId",
                table: "Installments",
                column: "TransactionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_LoanPeriods_LoanPeriodId",
                table: "Transactions",
                column: "LoanPeriodId",
                principalTable: "LoanPeriods",
                principalColumn: "LoanPeriodId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_LoanPeriods_LoanPeriodId",
                table: "Transactions");

            migrationBuilder.DropTable(
                name: "Installments");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_LoanPeriodId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "LoanPeriodId",
                table: "Transactions");
        }
    }
}
