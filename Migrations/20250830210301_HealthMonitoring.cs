using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SMS.Migrations
{
    /// <inheritdoc />
    public partial class HealthMonitoring : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Loans_TransactionId",
                table: "Loans");

            migrationBuilder.CreateTable(
                name: "BackupHistory",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BackupType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FileSize = table.Column<long>(type: "bigint", nullable: true),
                    Location = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BackupHistory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ServiceHealthStatus",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ResponseTime = table.Column<int>(type: "int", nullable: true),
                    LastChecked = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceHealthStatus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SystemHealthLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ComponentName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Metrics = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemHealthLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Loans_TransactionId",
                table: "Loans",
                column: "TransactionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BackupHistory_StartTime",
                table: "BackupHistory",
                column: "StartTime");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceHealthStatus_Checked",
                table: "ServiceHealthStatus",
                column: "LastChecked");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceHealthStatus_Service",
                table: "ServiceHealthStatus",
                column: "ServiceName");

            migrationBuilder.CreateIndex(
                name: "IX_SystemHealthLogs_Component",
                table: "SystemHealthLogs",
                column: "ComponentName");

            migrationBuilder.CreateIndex(
                name: "IX_SystemHealthLogs_Timestamp",
                table: "SystemHealthLogs",
                column: "Timestamp");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BackupHistory");

            migrationBuilder.DropTable(
                name: "ServiceHealthStatus");

            migrationBuilder.DropTable(
                name: "SystemHealthLogs");

            migrationBuilder.DropIndex(
                name: "IX_Loans_TransactionId",
                table: "Loans");

            migrationBuilder.CreateIndex(
                name: "IX_Loans_TransactionId",
                table: "Loans",
                column: "TransactionId");
        }
    }
}
