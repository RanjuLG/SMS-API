using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SMS.Migrations
{
    /// <inheritdoc />
    public partial class karat_ : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GoldCaratages");

            migrationBuilder.CreateTable(
                name: "Karats",
                columns: table => new
                {
                    KaratId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KaratValue = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Karats", x => x.KaratId);
                });

            migrationBuilder.CreateTable(
                name: "LoanPeriods",
                columns: table => new
                {
                    LoanPeriodId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Period = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoanPeriods", x => x.LoanPeriodId);
                });

            migrationBuilder.CreateTable(
                name: "Pricings",
                columns: table => new
                {
                    PricingId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    KaratId = table.Column<int>(type: "int", nullable: false),
                    LoanPeriodId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pricings", x => x.PricingId);
                    table.ForeignKey(
                        name: "FK_Pricings_Karats_KaratId",
                        column: x => x.KaratId,
                        principalTable: "Karats",
                        principalColumn: "KaratId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Pricings_LoanPeriods_LoanPeriodId",
                        column: x => x.LoanPeriodId,
                        principalTable: "LoanPeriods",
                        principalColumn: "LoanPeriodId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Pricings_KaratId",
                table: "Pricings",
                column: "KaratId");

            migrationBuilder.CreateIndex(
                name: "IX_Pricings_LoanPeriodId",
                table: "Pricings",
                column: "LoanPeriodId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Pricings");

            migrationBuilder.DropTable(
                name: "Karats");

            migrationBuilder.DropTable(
                name: "LoanPeriods");

            migrationBuilder.CreateTable(
                name: "GoldCaratages",
                columns: table => new
                {
                    CaratageId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Caratage = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<long>(type: "bigint", nullable: true),
                    OneMonth = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SixMonth = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ThreeMonth = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TwelveMonth = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GoldCaratages", x => x.CaratageId);
                });
        }
    }
}
