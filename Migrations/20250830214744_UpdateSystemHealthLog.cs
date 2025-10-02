using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SMS.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSystemHealthLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "CpuUsagePercentage",
                table: "SystemHealthLogs",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "DatabaseResponseTime",
                table: "SystemHealthLogs",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DatabaseStatus",
                table: "SystemHealthLogs",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "DatabaseStoragePercentage",
                table: "SystemHealthLogs",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "FilesStoragePercentage",
                table: "SystemHealthLogs",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "LogsStoragePercentage",
                table: "SystemHealthLogs",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "MemoryUsagePercentage",
                table: "SystemHealthLogs",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SystemStatus",
                table: "SystemHealthLogs",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CpuUsagePercentage",
                table: "SystemHealthLogs");

            migrationBuilder.DropColumn(
                name: "DatabaseResponseTime",
                table: "SystemHealthLogs");

            migrationBuilder.DropColumn(
                name: "DatabaseStatus",
                table: "SystemHealthLogs");

            migrationBuilder.DropColumn(
                name: "DatabaseStoragePercentage",
                table: "SystemHealthLogs");

            migrationBuilder.DropColumn(
                name: "FilesStoragePercentage",
                table: "SystemHealthLogs");

            migrationBuilder.DropColumn(
                name: "LogsStoragePercentage",
                table: "SystemHealthLogs");

            migrationBuilder.DropColumn(
                name: "MemoryUsagePercentage",
                table: "SystemHealthLogs");

            migrationBuilder.DropColumn(
                name: "SystemStatus",
                table: "SystemHealthLogs");
        }
    }
}
