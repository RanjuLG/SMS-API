using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SMS.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_Transactions_TransactionId",
                table: "Invoices");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_TransactionId",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "Date",
                table: "Transactions");

            migrationBuilder.AlterColumn<int>(
                name: "TransactionId",
                table: "Invoices",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "Carratages",
                columns: table => new
                {
                    CaratageId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Caratage = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AmountPerCaratage = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Carratages", x => x.CaratageId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_TransactionId",
                table: "Invoices",
                column: "TransactionId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_Transactions_TransactionId",
                table: "Invoices",
                column: "TransactionId",
                principalTable: "Transactions",
                principalColumn: "TransactionId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_Transactions_TransactionId",
                table: "Invoices");

            migrationBuilder.DropTable(
                name: "Carratages");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_TransactionId",
                table: "Invoices");

            migrationBuilder.AddColumn<DateTime>(
                name: "Date",
                table: "Transactions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "TransactionId",
                table: "Invoices",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_TransactionId",
                table: "Invoices",
                column: "TransactionId",
                unique: true,
                filter: "[TransactionId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_Transactions_TransactionId",
                table: "Invoices",
                column: "TransactionId",
                principalTable: "Transactions",
                principalColumn: "TransactionId");
        }
    }
}
