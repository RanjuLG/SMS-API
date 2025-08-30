namespace SMS.Models.Configuration
{
    public class HealthMonitoringConfiguration
    {
        public CacheTimeouts CacheTimeouts { get; set; } = new();
        public Thresholds Thresholds { get; set; } = new();
        public string[] Services { get; set; } = Array.Empty<string>();
        public BackupSettings BackupSettings { get; set; } = new();
    }

    public class CacheTimeouts
    {
        public int SystemHealth { get; set; } = 30;
        public int DatabaseHealth { get; set; } = 60;
        public int ServicesHealth { get; set; } = 45;
        public int BackupHealth { get; set; } = 300;
        public int StorageHealth { get; set; } = 120;
        public int SystemMetrics { get; set; } = 30;
        public int LogsHealth { get; set; } = 60;
        public int SecurityHealth { get; set; } = 1800;
    }

    public class Thresholds
    {
        public StorageThresholds Storage { get; set; } = new();
        public MetricsThresholds Metrics { get; set; } = new();
        public LogsThresholds Logs { get; set; } = new();
    }

    public class StorageThresholds
    {
        public double WarningPercentage { get; set; } = 70;
        public double CriticalPercentage { get; set; } = 90;
    }

    public class MetricsThresholds
    {
        public double CpuWarningPercentage { get; set; } = 70;
        public double CpuCriticalPercentage { get; set; } = 90;
        public double MemoryWarningPercentage { get; set; } = 70;
        public double MemoryCriticalPercentage { get; set; } = 90;
    }

    public class LogsThresholds
    {
        public int ErrorsWarningCount { get; set; } = 5;
        public int ErrorsCriticalCount { get; set; } = 10;
        public int WarningsNormalCount { get; set; } = 10;
        public int WarningsHighCount { get; set; } = 20;
    }

    public class BackupSettings
    {
        public int RetentionDays { get; set; } = 30;
        public string ScheduledTime { get; set; } = "02:00";
        public string Location { get; set; } = "local-storage";
    }
}
