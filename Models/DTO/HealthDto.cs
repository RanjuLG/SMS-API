using System.ComponentModel.DataAnnotations;

namespace SMS.Models.DTO
{
    public class SystemHealthDto
    {
        public string Status { get; set; } = "healthy";
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public long Uptime { get; set; }
        public string Version { get; set; } = "1.0.0";
    }

    public class DatabaseHealthDto
    {
        public string Status { get; set; } = "connected";
        public ConnectionPoolDto ConnectionPool { get; set; } = new();
        public double ResponseTime { get; set; }
        public DateTime LastChecked { get; set; } = DateTime.Now;
        public double Uptime { get; set; }
    }

    public class ConnectionPoolDto
    {
        public int Active { get; set; }
        public int Idle { get; set; }
        public int Max { get; set; }
    }

    public class ServicesHealthDto
    {
        public string Status { get; set; } = "operational";
        public List<ServiceHealthDto> Services { get; set; } = new();
        public double OverallUptime { get; set; }
    }

    public class ServiceHealthDto
    {
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = "operational";
        public double ResponseTime { get; set; }
        public DateTime LastChecked { get; set; } = DateTime.Now;
    }

    public class BackupHealthDto
    {
        public string Status { get; set; } = "success";
        public BackupInfoDto LastBackup { get; set; } = new();
        public DateTime NextScheduled { get; set; }
        public int RetentionDays { get; set; } = 30;
        public string BackupLocation { get; set; } = "local-storage";
    }

    public class BackupInfoDto
    {
        public DateTime Timestamp { get; set; }
        public int Duration { get; set; }
        public string Size { get; set; } = "0MB";
        public string Type { get; set; } = "full";
    }

    public class StorageHealthDto
    {
        public StorageComponentDto Database { get; set; } = new();
        public StorageComponentDto Files { get; set; } = new();
        public StorageComponentDto Logs { get; set; } = new();
    }

    public class StorageComponentDto
    {
        public string Used { get; set; } = "0MB";
        public string Total { get; set; } = "0MB";
        public double Percentage { get; set; }
        public string Status { get; set; } = "normal";
    }

    public class SystemMetricsDto
    {
        public CpuMetricsDto Cpu { get; set; } = new();
        public MemoryMetricsDto Memory { get; set; } = new();
        public NetworkMetricsDto Network { get; set; } = new();
    }

    public class CpuMetricsDto
    {
        public double Usage { get; set; }
        public int Cores { get; set; }
        public string Status { get; set; } = "normal";
    }

    public class MemoryMetricsDto
    {
        public string Used { get; set; } = "0MB";
        public string Total { get; set; } = "0MB";
        public double Percentage { get; set; }
        public string Status { get; set; } = "normal";
    }

    public class NetworkMetricsDto
    {
        public string Inbound { get; set; } = "0KB/s";
        public string Outbound { get; set; } = "0KB/s";
        public string Status { get; set; } = "normal";
    }

    public class LogsHealthDto
    {
        public LogMetricsDto Errors { get; set; } = new();
        public LogMetricsDto Warnings { get; set; } = new();
        public string LogLevel { get; set; } = "info";
        public LastErrorDto LastError { get; set; } = new();
    }

    public class LogMetricsDto
    {
        public int Last24h { get; set; }
        public int Last7d { get; set; }
        public string Status { get; set; } = "low";
    }

    public class LastErrorDto
    {
        public DateTime Timestamp { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Level { get; set; } = "info";
    }

    public class SecurityHealthDto
    {
        public string Status { get; set; } = "secure";
        public DateTime LastSecurityScan { get; set; }
        public VulnerabilitiesDto Vulnerabilities { get; set; } = new();
        public CertificatesDto Certificates { get; set; } = new();
    }

    public class VulnerabilitiesDto
    {
        public int Critical { get; set; }
        public int High { get; set; }
        public int Medium { get; set; }
        public int Low { get; set; }
    }

    public class CertificatesDto
    {
        public SslCertificateDto Ssl { get; set; } = new();
    }

    public class SslCertificateDto
    {
        public DateTime ExpiryDate { get; set; }
        public int DaysUntilExpiry { get; set; }
        public string Status { get; set; } = "valid";
    }

    public class HealthOverviewDto
    {
        public SystemHealthDto General { get; set; } = new();
        public DatabaseHealthSummaryDto Database { get; set; } = new();
        public ServicesHealthSummaryDto Services { get; set; } = new();
        public BackupHealthSummaryDto Backup { get; set; } = new();
        public StorageHealthSummaryDto Storage { get; set; } = new();
    }

    public class DatabaseHealthSummaryDto
    {
        public string Status { get; set; } = "connected";
        public double Uptime { get; set; }
        public double ResponseTime { get; set; }
    }

    public class ServicesHealthSummaryDto
    {
        public string Status { get; set; } = "operational";
        public double OverallUptime { get; set; }
    }

    public class BackupHealthSummaryDto
    {
        public string Status { get; set; } = "success";
        public BackupInfoDto LastBackup { get; set; } = new();
    }

    public class StorageHealthSummaryDto
    {
        public StorageComponentDto Database { get; set; } = new();
    }
}
