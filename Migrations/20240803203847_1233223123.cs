using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SMS.Migrations
{
    /// <inheritdoc />
    public partial class _1233223123 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ItemId",
                table: "TransactionItems",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransactionItems_ItemId",
                table: "TransactionItems",
                column: "ItemId1");

            migrationBuilder.AddForeignKey(
                name: "FK_TransactionItems_Items_ItemId1",
                table: "TransactionItems",
                column: "ItemId1",
                principalTable: "Items",
                principalColumn: "ItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TransactionItems_Items_ItemId",
                table: "TransactionItems");

            migrationBuilder.DropIndex(
                name: "IX_TransactionItems_ItemId",
                table: "TransactionItems");

            migrationBuilder.DropColumn(
                name: "ItemId1",
                table: "TransactionItems");
        }
    }
}
