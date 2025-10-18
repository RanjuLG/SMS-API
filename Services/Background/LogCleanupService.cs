using SMS.Models.Configuration;
using Microsoft.Extensions.Options;

namespace SMS.Services.Background
{
    public class LogCleanupService : BackgroundService
    {
        private readonly ILogger<LogCleanupService> _logger;
        private readonly HealthMonitoringConfiguration _config;
        private readonly TimeSpan _cleanupInterval = TimeSpan.FromHours(6); // Run every 6 hours

        public LogCleanupService(
            ILogger<LogCleanupService> logger,
            IOptions<HealthMonitoringConfiguration> config)
        {
            _logger = logger;
            _config = config.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Log Cleanup Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await PerformLogCleanupAsync();
                    await Task.Delay(_cleanupInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in log cleanup service");
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                }
            }

            _logger.LogInformation("Log Cleanup Service stopped");
        }

        private async Task PerformLogCleanupAsync()
        {
            try
            {
                var logsDirectory = Path.Combine(Directory.GetCurrentDirectory(), "logs");
                
                if (!Directory.Exists(logsDirectory))
                {
                    _logger.LogInformation("Logs directory does not exist: {LogsDirectory}", logsDirectory);
                    return;
                }

                var retentionDays = _config.BackupSettings.RetentionDays; // Use same retention as backups
                var cutoffDate = DateTime.Now.AddDays(-retentionDays);
                
                var logFiles = Directory.GetFiles(logsDirectory, "*.log", SearchOption.AllDirectories);
                var deletedCount = 0;
                var totalSizeDeleted = 0L;

                foreach (var logFile in logFiles)
                {
                    try
                    {
                        var fileInfo = new FileInfo(logFile);
                        
                        // Skip if file is too new
                        if (fileInfo.CreationTime >= cutoffDate && fileInfo.LastWriteTime >= cutoffDate)
                        {
                            continue;
                        }

                        // Skip currently active log files (modified in last hour)
                        if (fileInfo.LastWriteTime > DateTime.Now.AddHours(-1))
                        {
                            continue;
                        }

                        var fileSize = fileInfo.Length;
                        File.Delete(logFile);
                        
                        deletedCount++;
                        totalSizeDeleted += fileSize;
                        
                        _logger.LogDebug("Deleted old log file: {FileName}, Size: {Size:N0} bytes", 
                            fileInfo.Name, fileSize);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to delete log file: {LogFile}", logFile);
                    }
                }

                if (deletedCount > 0)
                {
                    _logger.LogInformation("Log cleanup completed: Deleted {Count} files, Freed {Size:N0} bytes ({SizeMB:F1} MB)",
                        deletedCount, totalSizeDeleted, totalSizeDeleted / 1024.0 / 1024.0);
                }
                else
                {
                    _logger.LogDebug("Log cleanup completed: No old log files to delete");
                }

                // Also cleanup large log files that might be causing disk space issues
                await CompressLargeLogFilesAsync(logsDirectory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to perform log cleanup");
            }
        }

        private async Task CompressLargeLogFilesAsync(string logsDirectory)
        {
            try
            {
                var logFiles = Directory.GetFiles(logsDirectory, "*.log", SearchOption.AllDirectories);
                const long maxFileSize = 50 * 1024 * 1024; // 50 MB
                
                foreach (var logFile in logFiles)
                {
                    try
                    {
                        var fileInfo = new FileInfo(logFile);
                        
                        // Skip if file is not large enough or is currently being written to
                        if (fileInfo.Length < maxFileSize || 
                            fileInfo.LastWriteTime > DateTime.Now.AddMinutes(-30))
                        {
                            continue;
                        }

                        // Archive the log file
                        await ArchiveLogFileAsync(logFile);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to archive large log file: {LogFile}", logFile);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to compress large log files");
            }
        }

        private async Task ArchiveLogFileAsync(string logFilePath)
        {
            try
            {
                var fileInfo = new FileInfo(logFilePath);
                var archiveDirectory = Path.Combine(Path.GetDirectoryName(logFilePath), "archived");
                Directory.CreateDirectory(archiveDirectory);
                
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var archiveFileName = $"{Path.GetFileNameWithoutExtension(logFilePath)}_{timestamp}.log";
                var archivePath = Path.Combine(archiveDirectory, archiveFileName);
                
                // Move file to archive directory
                File.Move(logFilePath, archivePath);
                
                _logger.LogInformation("Archived large log file: {OriginalFile} -> {ArchivedFile}, Size: {Size:N0} bytes",
                    fileInfo.Name, archiveFileName, fileInfo.Length);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to archive log file: {LogFile}", logFilePath);
            }
        }
    }
}
