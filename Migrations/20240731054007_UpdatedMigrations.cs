using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SMS.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedMigrations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Carratages",
                table: "Carratages");

            migrationBuilder.RenameTable(
                name: "Carratages",
                newName: "Caratages");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Caratages",
                table: "Caratages",
                column: "CaratageId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Caratages",
                table: "Caratages");

            migrationBuilder.RenameTable(
                name: "Caratages",
                newName: "Carratages");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Carratages",
                table: "Carratages",
                column: "CaratageId");
        }
    }
}
