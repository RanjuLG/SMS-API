using SMS.DBContext;
using SMS.Models;
using SMS.Models.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using System.Data.SqlClient;

namespace SMS.Services.Background
{
    public class DatabaseBackupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DatabaseBackupService> _logger;
        private readonly HealthMonitoringConfiguration _config;

        public DatabaseBackupService(
            IServiceProvider serviceProvider,
            ILogger<DatabaseBackupService> logger,
            IOptions<HealthMonitoringConfiguration> config)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _config = config.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Database Backup Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await WaitForScheduledTime(stoppingToken);
                    
                    if (!stoppingToken.IsCancellationRequested)
                    {
                        await PerformBackupAsync();
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in backup service execution");
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken); // Wait 1 hour before retry
                }
            }

            _logger.LogInformation("Database Backup Service stopped");
        }

        private async Task WaitForScheduledTime(CancellationToken stoppingToken)
        {
            var scheduledTime = TimeSpan.Parse(_config.BackupSettings.ScheduledTime);
            var now = DateTime.Now;
            var nextRun = DateTime.Today.Add(scheduledTime);

            // If we've already passed today's scheduled time, schedule for tomorrow
            if (nextRun <= now)
            {
                nextRun = nextRun.AddDays(1);
            }

            var delay = nextRun - now;
            _logger.LogInformation("Next backup scheduled for: {NextRun} (in {Delay})", 
                nextRun, delay);

            await Task.Delay(delay, stoppingToken);
        }

        private async Task PerformBackupAsync()
        {
            var backupHistory = new BackupHistory
            {
                BackupType = "full",
                Status = "running",
                StartTime = DateTime.UtcNow
            };

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                // Add backup record
                context.Set<BackupHistory>().Add(backupHistory);
                await context.SaveChangesAsync();

                _logger.LogInformation("Starting database backup...");

                // Generate backup file path
                var backupDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Backups");
                Directory.CreateDirectory(backupDirectory);

                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var backupFileName = $"SMS_Backup_{timestamp}.bak";
                var backupFilePath = Path.Combine(backupDirectory, backupFileName);

                // Execute SQL Server backup
                var connectionString = context.Database.GetConnectionString();
                var databaseName = GetDatabaseNameFromConnectionString(connectionString);

                await ExecuteSqlBackupAsync(connectionString, databaseName, backupFilePath);

                // Get backup file size
                var fileInfo = new FileInfo(backupFilePath);
                backupHistory.FileSize = fileInfo.Length;
                backupHistory.Location = backupFilePath;
                backupHistory.Status = "success";
                backupHistory.EndTime = DateTime.UtcNow;

                _logger.LogInformation("Backup completed successfully. File: {BackupFile}, Size: {Size:N0} bytes", 
                    backupFileName, fileInfo.Length);

                // Clean up old backups
                await CleanupOldBackupsAsync(backupDirectory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Backup failed");
                backupHistory.Status = "failed";
                backupHistory.EndTime = DateTime.UtcNow;
                backupHistory.ErrorMessage = ex.Message;
            }
            finally
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    context.Set<BackupHistory>().Update(backupHistory);
                    await context.SaveChangesAsync();
                }
                catch (Exception updateEx)
                {
                    _logger.LogError(updateEx, "Failed to update backup history");
                }
            }
        }

        private async Task ExecuteSqlBackupAsync(string connectionString, string databaseName, string backupPath)
        {
            var backupSql = $@"
                BACKUP DATABASE [{databaseName}] 
                TO DISK = @BackupPath
                WITH FORMAT, INIT, COMPRESSION,
                CHECKSUM, STOP_ON_ERROR";

            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand(backupSql, connection);
            command.Parameters.AddWithValue("@BackupPath", backupPath);
            command.CommandTimeout = 1800; // 30 minutes timeout

            await command.ExecuteNonQueryAsync();
        }

        private string GetDatabaseNameFromConnectionString(string connectionString)
        {
            var builder = new SqlConnectionStringBuilder(connectionString);
            return builder.InitialCatalog;
        }

        private async Task CleanupOldBackupsAsync(string backupDirectory)
        {
            try
            {
                var cutoffDate = DateTime.Now.AddDays(-_config.BackupSettings.RetentionDays);
                var backupFiles = Directory.GetFiles(backupDirectory, "*.bak");

                foreach (var file in backupFiles)
                {
                    var fileInfo = new FileInfo(file);
                    if (fileInfo.CreationTime < cutoffDate)
                    {
                        File.Delete(file);
                        _logger.LogInformation("Deleted old backup file: {FileName}", fileInfo.Name);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to cleanup old backup files");
            }
        }
    }
}
