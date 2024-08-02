using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SMS.Migrations
{
    /// <inheritdoc />
    public partial class @new : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Customers");

            migrationBuilder.RenameColumn(
                name: "LoanAmount",
                table: "Transactions",
                newName: "TotalAmount");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Transactions",
                newName: "TransactionId");

            migrationBuilder.AddColumn<decimal>(
                name: "SubTotal",
                table: "Transactions",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_CustomerId",
                table: "Transactions",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_ItemId",
                table: "Transactions",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Items_CustomerId",
                table: "Items",
                column: "CustomerId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_Items_Customers_CustomerId",
                table: "Items",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "CustomerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Customers_CustomerId",
                table: "Transactions",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "CustomerId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Items_ItemId",
                table: "Transactions",
                column: "ItemId",
                principalTable: "Items",
                principalColumn: "ItemId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_Transactions_TransactionId",
                table: "Invoices");

            migrationBuilder.DropForeignKey(
                name: "FK_Items_Customers_CustomerId",
                table: "Items");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Customers_CustomerId",
                table: "Transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Items_ItemId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_CustomerId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Transactions_ItemId",
                table: "Transactions");

            migrationBuilder.DropIndex(
                name: "IX_Items_CustomerId",
                table: "Items");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_TransactionId",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "SubTotal",
                table: "Transactions");

            migrationBuilder.RenameColumn(
                name: "TotalAmount",
                table: "Transactions",
                newName: "LoanAmount");

            migrationBuilder.RenameColumn(
                name: "TransactionId",
                table: "Transactions",
                newName: "Id");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Customers",
                type: "int",
                nullable: true);
        }
    }
}
