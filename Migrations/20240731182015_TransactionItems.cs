using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SMS.Migrations
{
    /// <inheritdoc />
    public partial class TransactionItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Items_ItemId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Transactions");

            migrationBuilder.RenameColumn(
                name: "AmountPerCaratage",
                table: "Caratages",
                newName: "TwelveMonth");

            migrationBuilder.AlterColumn<int>(
                name: "ItemId",
                table: "Transactions",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<decimal>(
                name: "OneMonth",
                table: "Caratages",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SixMonth",
                table: "Caratages",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ThreeMonth",
                table: "Caratages",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Items_ItemId",
                table: "Transactions",
                column: "ItemId",
                principalTable: "Items",
                principalColumn: "ItemId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Items_ItemId",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "OneMonth",
                table: "Caratages");

            migrationBuilder.DropColumn(
                name: "SixMonth",
                table: "Caratages");

            migrationBuilder.DropColumn(
                name: "ThreeMonth",
                table: "Caratages");

            migrationBuilder.RenameColumn(
                name: "TwelveMonth",
                table: "Caratages",
                newName: "AmountPerCaratage");

            migrationBuilder.AlterColumn<int>(
                name: "ItemId",
                table: "Transactions",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Transactions",
                type: "int",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_Items_ItemId",
                table: "Transactions",
                column: "ItemId",
                principalTable: "Items",
                principalColumn: "ItemId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
