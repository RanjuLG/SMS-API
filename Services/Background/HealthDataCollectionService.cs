using SMS.DBContext;
using SMS.Interfaces;
using SMS.Models;
using SMS.Models.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;

namespace SMS.Services.Background
{
    public class HealthDataCollectionService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<HealthDataCollectionService> _logger;
        private readonly HealthMonitoringConfiguration _config;
        private readonly TimeSpan _collectionInterval = TimeSpan.FromMinutes(5); // Collect every 5 minutes

        public HealthDataCollectionService(
            IServiceProvider serviceProvider,
            ILogger<HealthDataCollectionService> logger,
            IOptions<HealthMonitoringConfiguration> config)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _config = config.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Health Data Collection Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CollectHealthDataAsync();
                    await Task.Delay(_collectionInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error collecting health data");
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken); // Wait 1 minute before retry
                }
            }

            _logger.LogInformation("Health Data Collection Service stopped");
        }

        private async Task CollectHealthDataAsync()
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var healthService = scope.ServiceProvider.GetRequiredService<IHealthService>();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                // Collect system health data
                var systemHealth = await healthService.GetSystemHealthAsync();
                var databaseHealth = await healthService.GetDatabaseHealthAsync();
                var storageHealth = await healthService.GetStorageHealthAsync();
                var metricsHealth = await healthService.GetSystemMetricsAsync();

                // Create health log entry
                var healthLog = new SystemHealthLog
                {
                    Timestamp = DateTime.UtcNow,
                    SystemStatus = systemHealth.Status,
                    DatabaseStatus = databaseHealth.Status,
                    DatabaseResponseTime = databaseHealth.ResponseTime,
                    CpuUsagePercentage = metricsHealth.Cpu.Usage,
                    MemoryUsagePercentage = metricsHealth.Memory.Percentage,
                    DatabaseStoragePercentage = storageHealth.Database.Percentage,
                    FilesStoragePercentage = storageHealth.Files.Percentage,
                    LogsStoragePercentage = storageHealth.Logs.Percentage
                };

                context.Set<SystemHealthLog>().Add(healthLog);
                await context.SaveChangesAsync();

                _logger.LogDebug("Health data collected: System={SystemStatus}, DB={DatabaseStatus}, CPU={CpuUsage}%, Memory={MemoryUsage}%",
                    systemHealth.Status, databaseHealth.Status, metricsHealth.Cpu.Usage, metricsHealth.Memory.Percentage);

                // Cleanup old health logs (keep last 30 days)
                await CleanupOldHealthLogsAsync(context);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to collect health data");
            }
        }

        private async Task CleanupOldHealthLogsAsync(ApplicationDbContext context)
        {
            try
            {
                var cutoffDate = DateTime.UtcNow.AddDays(-30);
                var oldLogs = context.Set<SystemHealthLog>()
                    .Where(log => log.Timestamp < cutoffDate);

                var count = await oldLogs.CountAsync();
                if (count > 0)
                {
                    context.Set<SystemHealthLog>().RemoveRange(oldLogs);
                    await context.SaveChangesAsync();
                    _logger.LogInformation("Cleaned up {Count} old health log entries", count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to cleanup old health logs");
            }
        }
    }
}
