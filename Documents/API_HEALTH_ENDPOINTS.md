# System Health Monitoring API Documentation

## Overview

This document outlines the API endpoints required for implementing comprehensive system health monitoring in the SMS (Shop Management System) application. These endpoints will provide real-time monitoring capabilities for the jewelry shop management dashboard.

## Base URL
```
{API_BASE_URL}/api/health
```

## Authentication
All endpoints require JWT authentication. Include the Authorization header:
```
Authorization: Bearer <jwt_token>
```

## API Endpoints

### 1. General System Health

**Endpoint:** `GET /api/health`

**Description:** Returns overall system health status and basic metrics.

**Response Example:**
```json
{
  "status": "healthy",
  "timestamp": "2025-08-31T10:30:00Z",
  "uptime": 86400,
  "version": "1.0.0"
}
```

**Status Values:**
- `healthy`: All systems operational
- `degraded`: Some non-critical issues
- `unhealthy`: Critical issues detected

---

### 2. Database Health

**Endpoint:** `GET /api/health/database`

**Description:** Database connection status and performance metrics.

**Response Example:**
```json
{
  "status": "connected",
  "connectionPool": {
    "active": 5,
    "idle": 10,
    "max": 20
  },
  "responseTime": 45,
  "lastChecked": "2025-08-31T10:30:00Z",
  "uptime": 99.9
}
```

**Implementation Notes:**
- Monitor connection pool health
- Track query response times
- Check database connectivity
- Calculate uptime percentage

---

### 3. API Services Health

**Endpoint:** `GET /api/health/services`

**Description:** Health status of individual microservices or API endpoints.

**Response Example:**
```json
{
  "status": "operational",
  "services": [
    {
      "name": "auth-service",
      "status": "operational",
      "responseTime": 120,
      "lastChecked": "2025-08-31T10:30:00Z"
    },
    {
      "name": "invoice-service",
      "status": "operational",
      "responseTime": 89,
      "lastChecked": "2025-08-31T10:30:00Z"
    },
    {
      "name": "customer-service",
      "status": "operational",
      "responseTime": 95,
      "lastChecked": "2025-08-31T10:30:00Z"
    },
    {
      "name": "inventory-service",
      "status": "operational",
      "responseTime": 78,
      "lastChecked": "2025-08-31T10:30:00Z"
    },
    {
      "name": "reports-service",
      "status": "operational",
      "responseTime": 156,
      "lastChecked": "2025-08-31T10:30:00Z"
    }
  ],
  "overallUptime": 100
}
```

**Service Status Values:**
- `operational`: Service working normally
- `degraded`: Service slow but functional
- `down`: Service unavailable

---

### 4. Backup Status

**Endpoint:** `GET /api/health/backup`

**Description:** Database backup status and schedule information.

**Response Example:**
```json
{
  "status": "success",
  "lastBackup": {
    "timestamp": "2025-08-31T08:30:00Z",
    "duration": 1800,
    "size": "2.4GB",
    "type": "full"
  },
  "nextScheduled": "2025-09-01T02:00:00Z",
  "retentionDays": 30,
  "backupLocation": "cloud-storage"
}
```

**Backup Types:**
- `full`: Complete database backup
- `incremental`: Changes since last backup

**Status Values:**
- `success`: Last backup completed successfully
- `failed`: Last backup failed
- `in-progress`: Backup currently running

---

### 5. Storage Usage

**Endpoint:** `GET /api/health/storage`

**Description:** Disk space usage for different components.

**Response Example:**
```json
{
  "database": {
    "used": "1.2GB",
    "total": "5GB",
    "percentage": 24,
    "status": "normal"
  },
  "files": {
    "used": "850MB",
    "total": "2GB",
    "percentage": 42.5,
    "status": "normal"
  },
  "logs": {
    "used": "150MB",
    "total": "500MB",
    "percentage": 30,
    "status": "normal"
  }
}
```

**Storage Status Thresholds:**
- `normal`: < 70% usage
- `warning`: 70-90% usage
- `critical`: > 90% usage

---

### 6. System Metrics

**Endpoint:** `GET /api/health/metrics`

**Description:** Server performance metrics (CPU, Memory, Network).

**Response Example:**
```json
{
  "cpu": {
    "usage": 45.2,
    "cores": 4,
    "status": "normal"
  },
  "memory": {
    "used": "2.1GB",
    "total": "8GB",
    "percentage": 26.25,
    "status": "normal"
  },
  "network": {
    "inbound": "1.2MB/s",
    "outbound": "800KB/s",
    "status": "normal"
  }
}
```

**Metric Status Thresholds:**
- `normal`: Within acceptable limits
- `warning`: Approaching limits
- `critical`: Exceeding safe limits

---

### 7. Application Logs

**Endpoint:** `GET /api/health/logs`

**Description:** Application error and warning summary.

**Response Example:**
```json
{
  "errors": {
    "last24h": 2,
    "last7d": 15,
    "status": "low"
  },
  "warnings": {
    "last24h": 8,
    "last7d": 45,
    "status": "normal"
  },
  "logLevel": "info",
  "lastError": {
    "timestamp": "2025-08-31T09:15:00Z",
    "message": "Database connection timeout",
    "level": "error"
  }
}
```

**Error Status Levels:**
- `low`: Minimal error rate
- `medium`: Moderate error rate
- `high`: High error rate (investigation needed)

---

### 8. Security Status

**Endpoint:** `GET /api/health/security`

**Description:** Security monitoring and certificate status.

**Response Example:**
```json
{
  "status": "secure",
  "lastSecurityScan": "2025-08-30T02:00:00Z",
  "vulnerabilities": {
    "critical": 0,
    "high": 0,
    "medium": 2,
    "low": 5
  },
  "certificates": {
    "ssl": {
      "expiryDate": "2026-08-31T23:59:59Z",
      "daysUntilExpiry": 365,
      "status": "valid"
    }
  }
}
```

**Security Status Values:**
- `secure`: No critical vulnerabilities
- `warning`: Some vulnerabilities detected
- `critical`: Critical vulnerabilities found

---

### 9. System Health Overview

**Endpoint:** `GET /api/health/overview`

**Description:** Comprehensive health status combining all metrics.

**Response Example:**
```json
{
  "general": {
    "status": "healthy",
    "timestamp": "2025-08-31T10:30:00Z",
    "uptime": 86400,
    "version": "1.0.0"
  },
  "database": {
    "status": "connected",
    "uptime": 99.9,
    "responseTime": 45
  },
  "services": {
    "status": "operational",
    "overallUptime": 100
  },
  "backup": {
    "status": "success",
    "lastBackup": {
      "timestamp": "2025-08-31T08:30:00Z",
      "type": "full"
    }
  },
  "storage": {
    "database": {
      "percentage": 24,
      "status": "normal"
    }
  }
}
```

## Implementation Guidelines

### 1. Caching Strategy
- Cache health data for 30-60 seconds to avoid excessive system calls
- Implement cache invalidation for critical status changes

### 2. Error Handling
- Return appropriate HTTP status codes (200, 503, etc.)
- Include error details in response body
- Log all health check requests and responses

### 3. Security Considerations
- Limit sensitive information exposure
- Implement rate limiting for health endpoints
- Audit health endpoint access

### 4. Performance Monitoring
- Keep health checks lightweight
- Monitor health endpoint performance
- Set reasonable timeouts for external checks

### 5. Alerting Integration
- Trigger alerts based on status changes
- Implement webhook notifications for critical issues
- Log significant health status changes

## Database Schema Requirements

Consider creating these tables for health monitoring:

```sql
-- System Health Logs
CREATE TABLE system_health_logs (
    id BIGINT PRIMARY KEY IDENTITY(1,1),
    component_name NVARCHAR(100) NOT NULL,
    status NVARCHAR(50) NOT NULL,
    metrics NVARCHAR(MAX), -- JSON data
    timestamp DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    INDEX IX_system_health_logs_timestamp (timestamp),
    INDEX IX_system_health_logs_component (component_name)
);

-- Backup History
CREATE TABLE backup_history (
    id BIGINT PRIMARY KEY IDENTITY(1,1),
    backup_type NVARCHAR(50) NOT NULL,
    status NVARCHAR(50) NOT NULL,
    start_time DATETIME2 NOT NULL,
    end_time DATETIME2,
    file_size BIGINT,
    location NVARCHAR(500),
    error_message NVARCHAR(MAX),
    INDEX IX_backup_history_start_time (start_time)
);

-- Service Health Status
CREATE TABLE service_health_status (
    id BIGINT PRIMARY KEY IDENTITY(1,1),
    service_name NVARCHAR(100) NOT NULL,
    status NVARCHAR(50) NOT NULL,
    response_time INT,
    last_checked DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    error_message NVARCHAR(MAX),
    INDEX IX_service_health_status_service (service_name),
    INDEX IX_service_health_status_checked (last_checked)
);
```

## Testing Endpoints

Use these sample requests to test the endpoints:

```bash
# Test general health
curl -H "Authorization: Bearer <token>" \
     GET "{API_BASE_URL}/api/health"

# Test database health
curl -H "Authorization: Bearer <token>" \
     GET "{API_BASE_URL}/api/health/database"

# Test services health
curl -H "Authorization: Bearer <token>" \
     GET "{API_BASE_URL}/api/health/services"
```

## Frontend Integration

The Angular service methods are already implemented in `ApiService`:

```typescript
// Usage in components
this.apiService.getSystemHealth().subscribe(health => {
  // Update UI with health status
});

this.apiService.getSystemHealthOverview().subscribe(overview => {
  // Update dashboard with comprehensive health data
});
```
