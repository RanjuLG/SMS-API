using SMS.Interfaces;
using SMS.Models.Configuration;
using Microsoft.Extensions.Options;

namespace SMS.Services.Background
{
    public class HealthAlertingService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<HealthAlertingService> _logger;
        private readonly HealthMonitoringConfiguration _config;
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(2); // Check every 2 minutes

        private readonly Dictionary<string, DateTime> _lastAlertTimes = new();
        private readonly TimeSpan _alertCooldown = TimeSpan.FromMinutes(15); // Don't spam alerts

        public HealthAlertingService(
            IServiceProvider serviceProvider,
            ILogger<HealthAlertingService> logger,
            IOptions<HealthMonitoringConfiguration> config)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _config = config.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Health Alerting Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckHealthAndSendAlertsAsync();
                    await Task.Delay(_checkInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in health alerting service");
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
            }

            _logger.LogInformation("Health Alerting Service stopped");
        }

        private async Task CheckHealthAndSendAlertsAsync()
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var healthService = scope.ServiceProvider.GetRequiredService<IHealthService>();

                var alerts = new List<HealthAlert>();

                // Check system health
                var systemHealth = await healthService.GetSystemHealthAsync();
                if (systemHealth.Status == "unhealthy")
                {
                    alerts.Add(new HealthAlert
                    {
                        Level = AlertLevel.Critical,
                        Component = "System",
                        Message = "System is unhealthy",
                        Details = $"System status: {systemHealth.Status}"
                    });
                }

                // Check database health
                var databaseHealth = await healthService.GetDatabaseHealthAsync();
                if (databaseHealth.Status != "connected")
                {
                    alerts.Add(new HealthAlert
                    {
                        Level = AlertLevel.Critical,
                        Component = "Database",
                        Message = "Database connection issues",
                        Details = $"Database status: {databaseHealth.Status}, Response time: {databaseHealth.ResponseTime}ms"
                    });
                }
                else if (databaseHealth.ResponseTime > 5000) // 5 seconds
                {
                    alerts.Add(new HealthAlert
                    {
                        Level = AlertLevel.Warning,
                        Component = "Database",
                        Message = "Database response time is high",
                        Details = $"Response time: {databaseHealth.ResponseTime}ms"
                    });
                }

                // Check storage health
                var storageHealth = await healthService.GetStorageHealthAsync();
                
                if (storageHealth.Database.Status == "critical")
                {
                    alerts.Add(new HealthAlert
                    {
                        Level = AlertLevel.Critical,
                        Component = "Storage",
                        Message = "Database storage critically low",
                        Details = $"Database storage: {storageHealth.Database.Percentage}% used"
                    });
                }

                if (storageHealth.Files.Status == "critical")
                {
                    alerts.Add(new HealthAlert
                    {
                        Level = AlertLevel.Critical,
                        Component = "Storage",
                        Message = "File storage critically low",
                        Details = $"File storage: {storageHealth.Files.Percentage}% used"
                    });
                }

                // Check system metrics
                var metrics = await healthService.GetSystemMetricsAsync();
                
                if (metrics.Cpu.Status == "critical")
                {
                    alerts.Add(new HealthAlert
                    {
                        Level = AlertLevel.Critical,
                        Component = "CPU",
                        Message = "CPU usage critically high",
                        Details = $"CPU usage: {metrics.Cpu.Usage}%"
                    });
                }

                if (metrics.Memory.Status == "critical")
                {
                    alerts.Add(new HealthAlert
                    {
                        Level = AlertLevel.Critical,
                        Component = "Memory",
                        Message = "Memory usage critically high",
                        Details = $"Memory usage: {metrics.Memory.Percentage}%"
                    });
                }

                // Check backup status
                var backupHealth = await healthService.GetBackupHealthAsync();
                if (backupHealth.Status == "failed")
                {
                    alerts.Add(new HealthAlert
                    {
                        Level = AlertLevel.Warning,
                        Component = "Backup",
                        Message = "Latest backup failed",
                        Details = "Database backup system requires attention"
                    });
                }
                else if (backupHealth.Status == "no-backup")
                {
                    alerts.Add(new HealthAlert
                    {
                        Level = AlertLevel.Warning,
                        Component = "Backup",
                        Message = "No recent backups found",
                        Details = "Database backup system may not be configured"
                    });
                }

                // Check logs health
                var logsHealth = await healthService.GetLogsHealthAsync();
                if (logsHealth.Errors.Status == "high")
                {
                    alerts.Add(new HealthAlert
                    {
                        Level = AlertLevel.Warning,
                        Component = "Logs",
                        Message = "High number of application errors",
                        Details = $"Errors in last 24h: {logsHealth.Errors.Last24h}"
                    });
                }

                // Send alerts
                foreach (var alert in alerts)
                {
                    await SendAlertIfNeededAsync(alert);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to check health for alerting");
            }
        }

        private async Task SendAlertIfNeededAsync(HealthAlert alert)
        {
            var alertKey = $"{alert.Component}_{alert.Level}";
            
            // Check if we've sent this alert recently (cooldown)
            if (_lastAlertTimes.TryGetValue(alertKey, out var lastAlert))
            {
                if (DateTime.UtcNow - lastAlert < _alertCooldown)
                {
                    return; // Skip - too soon to send same alert again
                }
            }

            try
            {
                // Log the alert
                var logLevel = alert.Level switch
                {
                    AlertLevel.Critical => LogLevel.Critical,
                    AlertLevel.Warning => LogLevel.Warning,
                    _ => LogLevel.Information
                };

                _logger.Log(logLevel, "HEALTH ALERT: {Message} - {Details}", alert.Message, alert.Details);

                // Here you could add actual alerting mechanisms:
                // - Send email notifications
                // - Send SMS alerts
                // - Post to Slack/Teams
                // - Send webhook notifications
                // - Write to Windows Event Log

                // For now, we'll just log to the application log
                await SendConsoleAlertAsync(alert);

                // Update last alert time
                _lastAlertTimes[alertKey] = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send health alert: {Message}", alert.Message);
            }
        }

        private async Task SendConsoleAlertAsync(HealthAlert alert)
        {
            var alertMessage = $"""
                ===============================================
                🚨 HEALTH ALERT - {alert.Level.ToString().ToUpper()}
                ===============================================
                Component: {alert.Component}
                Message: {alert.Message}
                Details: {alert.Details}
                Time: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC
                ===============================================
                """;

            Console.WriteLine(alertMessage);
            
            // Also write to a dedicated alerts log file
            var alertsLogPath = Path.Combine("logs", "health-alerts.log");
            Directory.CreateDirectory(Path.GetDirectoryName(alertsLogPath));
            
            await File.AppendAllTextAsync(alertsLogPath, alertMessage + Environment.NewLine);
        }
    }

    public class HealthAlert
    {
        public AlertLevel Level { get; set; }
        public string Component { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    public enum AlertLevel
    {
        Information,
        Warning,
        Critical
    }
}
