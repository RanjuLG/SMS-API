using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SMS.Migrations
{
    /// <inheritdoc />
    public partial class loan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Installments_Transactions_TransactionId",
                table: "Installments");

            migrationBuilder.AddColumn<int>(
                name: "LoanId",
                table: "Installments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Loans",
                columns: table => new
                {
                    LoanId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TransactionId = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Loans", x => x.LoanId);
                    table.ForeignKey(
                        name: "FK_Loans_Transactions_TransactionId",
                        column: x => x.TransactionId,
                        principalTable: "Transactions",
                        principalColumn: "TransactionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Installments_LoanId",
                table: "Installments",
                column: "LoanId");

            migrationBuilder.CreateIndex(
                name: "IX_Loans_TransactionId",
                table: "Loans",
                column: "TransactionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Installments_Loans_LoanId",
                table: "Installments",
                column: "LoanId",
                principalTable: "Loans",
                principalColumn: "LoanId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Installments_Transactions_TransactionId",
                table: "Installments",
                column: "TransactionId",
                principalTable: "Transactions",
                principalColumn: "TransactionId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Installments_Loans_LoanId",
                table: "Installments");

            migrationBuilder.DropForeignKey(
                name: "FK_Installments_Transactions_TransactionId",
                table: "Installments");

            migrationBuilder.DropTable(
                name: "Loans");

            migrationBuilder.DropIndex(
                name: "IX_Installments_LoanId",
                table: "Installments");

            migrationBuilder.DropColumn(
                name: "LoanId",
                table: "Installments");

            migrationBuilder.AddForeignKey(
                name: "FK_Installments_Transactions_TransactionId",
                table: "Installments",
                column: "TransactionId",
                principalTable: "Transactions",
                principalColumn: "TransactionId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
