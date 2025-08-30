# SMS API Health Monitoring - Background Services

This document describes the background services that provide automated monitoring and maintenance for the SMS API health monitoring system.

## Background Services Overview

The health monitoring system includes 5 background services that run continuously to ensure optimal system performance and proactive monitoring:

### 1. DatabaseBackupService
**Purpose**: Automated database backup execution with scheduling
**Schedule**: Daily at 2:00 AM
**Features**:
- SQL Server database backup automation
- Configurable backup retention (default: 30 days)
- Backup history logging
- Old backup cleanup
- Backup verification

**Configuration** (appsettings.json):
```json
{
  "HealthMonitoring": {
    "DatabaseBackup": {
      "Enabled": true,
      "BackupPath": "C:\\DatabaseBackups\\SMS",
      "RetentionDays": 30,
      "ScheduleTime": "02:00"
    }
  }
}
```

### 2. HealthDataCollectionService
**Purpose**: Historical health data collection for trending and analysis
**Schedule**: Every 5 minutes
**Features**:
- Collects system metrics (CPU, memory, disk, network)
- Stores data in SystemHealthLog table
- Configurable retention period (default: 90 days)
- Provides data for health trends and analytics

**Database Table**: `SystemHealthLogs`
- Stores timestamped health metrics
- Used for historical analysis and trending
- Automatically cleaned up based on retention settings

### 3. HealthAlertingService
**Purpose**: Automated health monitoring and alerting system
**Schedule**: Every 2 minutes
**Features**:
- Monitors CPU usage thresholds (>80% warning, >90% critical)
- Monitors memory usage thresholds (>80% warning, >90% critical)
- Monitors disk space thresholds (<20% warning, <10% critical)
- Alert cooldown periods to prevent spam
- Console and file-based alerting

**Alert Levels**:
- **INFO**: Normal operation
- **WARNING**: Requires attention (80-90% thresholds)
- **CRITICAL**: Immediate action required (>90% or <10% disk)

### 4. LogCleanupService
**Purpose**: Automated log file management and cleanup
**Schedule**: Daily at 3:00 AM
**Features**:
- Log file retention management (default: 30 days)
- Large file archiving (>100MB files)
- Disk space monitoring
- Configurable cleanup policies

**Managed Log Files**:
- Application logs (`logs/sms-api-*.log`)
- Error logs (`logs/sms-api-errors-*.log`)
- Health alert logs (`logs/health-alerts.log`)
- Certificate alert logs (`logs/certificate-alerts.log`)

### 5. CertificateMonitoringService
**Purpose**: SSL/TLS certificate expiry monitoring
**Schedule**: Every 12 hours (twice daily)
**Features**:
- Monitors certificates in Windows certificate stores
- Checks application certificate files (.cer, .crt, .pem, .pfx)
- Certificate expiry alerts (90, 30, 7 days, and expired)
- Server authentication certificate detection

**Alert Thresholds**:
- **90 days**: Info level
- **30 days**: Warning level
- **7 days**: Critical level
- **Expired**: Critical alert

## Service Management

### Starting Services
All background services are automatically registered in `Program.cs` and start with the application:

```csharp
builder.Services.AddHostedService<DatabaseBackupService>();
builder.Services.AddHostedService<HealthDataCollectionService>();
builder.Services.AddHostedService<HealthAlertingService>();
builder.Services.AddHostedService<LogCleanupService>();
builder.Services.AddHostedService<CertificateMonitoringService>();
```

### Monitoring Service Status
Services can be monitored through:
1. Application logs (Serilog)
2. Health endpoints (/api/health/services)
3. Console output during development
4. Alert log files

### Configuration
All services are configured through the `HealthMonitoring` section in `appsettings.json`:

```json
{
  "HealthMonitoring": {
    "DatabaseBackup": {
      "Enabled": true,
      "BackupPath": "C:\\DatabaseBackups\\SMS",
      "RetentionDays": 30,
      "ScheduleTime": "02:00"
    },
    "Alerts": {
      "CpuThresholdWarning": 80,
      "CpuThresholdCritical": 90,
      "MemoryThresholdWarning": 80,
      "MemoryThresholdCritical": 90,
      "DiskSpaceThresholdWarning": 20,
      "DiskSpaceThresholdCritical": 10,
      "AlertCooldownMinutes": 30
    },
    "LogCleanup": {
      "RetentionDays": 30,
      "LargeFileSizeMB": 100,
      "CleanupScheduleTime": "03:00"
    },
    "HealthCollection": {
      "CollectionIntervalMinutes": 5,
      "RetentionDays": 90
    }
  }
}
```

## Deployment Considerations

### Production Deployment
1. **Database Backup Path**: Ensure the backup path exists and has sufficient disk space
2. **Service Account**: Ensure the application runs with appropriate permissions for:
   - Database backup operations
   - Certificate store access
   - File system access for logs and backups
3. **Disk Space**: Monitor disk space for backup and log storage
4. **Network**: Ensure the application can access external health check endpoints

### Performance Impact
- **CPU Usage**: Background services are designed to have minimal CPU impact
- **Memory Usage**: Services use efficient data structures and dispose resources properly
- **I/O Operations**: File operations are performed asynchronously to avoid blocking
- **Database Impact**: Health data collection uses minimal database resources

### Troubleshooting
Common issues and solutions:

1. **Database Backup Failures**:
   - Check SQL Server permissions
   - Verify backup path accessibility
   - Monitor disk space

2. **Certificate Monitoring Issues**:
   - Verify certificate store permissions
   - Check for certificate file access
   - Review certificate store locations

3. **High Resource Usage**:
   - Review collection intervals
   - Check log retention settings
   - Monitor background service performance

## Logging and Monitoring

### Log Locations
- **Application Logs**: `logs/sms-api-{date}.log`
- **Error Logs**: `logs/sms-api-errors-{date}.log`
- **Health Alerts**: `logs/health-alerts.log`
- **Certificate Alerts**: `logs/certificate-alerts.log`
- **Database Backups**: `logs/database-backup.log`

### Service Health Checks
Use the health endpoints to monitor background service status:
- `GET /api/health/services` - Overall service health
- `GET /api/health/overview` - Complete system overview
- `GET /api/health/logs` - Recent log analysis

### Maintenance
Regular maintenance tasks:
1. Review alert logs weekly
2. Monitor backup success monthly
3. Check certificate expiry quarterly
4. Review disk space usage monthly
5. Update thresholds based on system performance

This comprehensive background service system ensures your SMS API operates reliably with proactive monitoring and automated maintenance tasks.
