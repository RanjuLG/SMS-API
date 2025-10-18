using SMS.Models.DTO;

namespace SMS.Interfaces
{
    public interface IHealthService
    {
        Task<SystemHealthDto> GetSystemHealthAsync();
        Task<DatabaseHealthDto> GetDatabaseHealthAsync();
        Task<ServicesHealthDto> GetServicesHealthAsync();
        Task<BackupHealthDto> GetBackupHealthAsync();
        Task<StorageHealthDto> GetStorageHealthAsync();
        Task<SystemMetricsDto> GetSystemMetricsAsync();
        Task<LogsHealthDto> GetLogsHealthAsync();
        Task<SecurityHealthDto> GetSecurityHealthAsync();
        Task<HealthOverviewDto> GetHealthOverviewAsync();
        Task LogHealthStatusAsync(string componentName, string status, object? metrics = null);
    }
}
