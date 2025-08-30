using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SMS.Models
{
    [Table("SystemHealthLogs")]
    public class SystemHealthLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string ComponentName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = string.Empty;

        [Column(TypeName = "nvarchar(max)")]
        public string? Metrics { get; set; }

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        // System Health Properties
        [MaxLength(50)]
        public string? SystemStatus { get; set; }

        [MaxLength(50)]
        public string? DatabaseStatus { get; set; }

        public double? DatabaseResponseTime { get; set; }

        public double? CpuUsagePercentage { get; set; }

        public double? MemoryUsagePercentage { get; set; }

        public double? DatabaseStoragePercentage { get; set; }

        public double? FilesStoragePercentage { get; set; }

        public double? LogsStoragePercentage { get; set; }
    }

    [Table("BackupHistory")]
    public class BackupHistory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string BackupType { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = string.Empty;

        [Required]
        public DateTime StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public long? FileSize { get; set; }

        [MaxLength(500)]
        public string? Location { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string? ErrorMessage { get; set; }
    }

    [Table("ServiceHealthStatus")]
    public class ServiceHealthStatus
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string ServiceName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = string.Empty;

        public int? ResponseTime { get; set; }

        [Required]
        public DateTime LastChecked { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "nvarchar(max)")]
        public string? ErrorMessage { get; set; }
    }
}
