# Health Monitoring API - Frontend Documentation

## Overview
The Health Monitoring API provides comprehensive system health endpoints for frontend integration. All endpoints (except `/ping`) require JWT authentication.

**Base URL**: `/api/health`

**Authentication**: Bearer Token (JWT) required for all endpoints except `/ping`

**Content-Type**: `application/json`

---

## 🔐 Authentication

### Headers Required
```javascript
{
  "Authorization": "Bearer YOUR_JWT_TOKEN",
  "Content-Type": "application/json"
}
```

### Roles & Access
- **All Health Endpoints**: Requires authentication (SuperAdmin, Admin, Cashier)
- **Security Endpoint**: Requires Admin role (SuperAdmin, Admin only)
- **Ping Endpoint**: Anonymous access allowed

---

## 📊 API Endpoints

### 1. **System Health Overview**
**`GET /api/health`**

Get overall system health status and basic metrics.

#### Response Status Codes
- `200 OK` - System is healthy
- `503 Service Unavailable` - System is unhealthy
- `401 Unauthorized` - Invalid or missing JWT token

#### Success Response (200)
```json
{
  "status": "healthy",
  "timestamp": "2025-08-31T10:30:00Z",
  "version": "1.0.0",
  "uptime": 86400,
  "environment": "Production"
}
```

#### Error Response (503)
```json
{
  "status": "unhealthy",
  "timestamp": "2025-08-31T10:30:00Z",
  "version": "1.0.0",
  "uptime": 86400,
  "environment": "Production"
}
```

#### Frontend Implementation
```typescript
interface SystemHealthDto {
  status: 'healthy' | 'degraded' | 'unhealthy';
  timestamp: string;
  version: string;
  uptime: number;
  environment: string;
}

// Usage
async function getSystemHealth(): Promise<SystemHealthDto> {
  const response = await fetch('/api/health', {
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    }
  });
  
  if (!response.ok) {
    throw new Error(`Health check failed: ${response.status}`);
  }
  
  return await response.json();
}
```

---

### 2. **Database Health**
**`GET /api/health/database`**

Get database connection status and performance metrics.

#### Success Response (200)
```json
{
  "status": "connected",
  "responseTime": 45,
  "lastChecked": "2025-08-31T10:30:00Z",
  "uptime": 99.9,
  "connectionPool": {
    "active": 5,
    "idle": 10,
    "max": 100
  }
}
```

#### Frontend Implementation
```typescript
interface DatabaseHealthDto {
  status: 'connected' | 'disconnected' | 'error';
  responseTime: number;
  lastChecked: string;
  uptime: number;
  connectionPool: {
    active: number;
    idle: number;
    max: number;
  };
}
```

---

### 3. **Services Health**
**`GET /api/health/services`**

Get health status of individual application services.

#### Success Response (200)
```json
{
  "status": "healthy",
  "services": [
    {
      "name": "auth-service",
      "status": "healthy",
      "responseTime": 12,
      "lastChecked": "2025-08-31T10:30:00Z"
    },
    {
      "name": "invoice-service",
      "status": "healthy",
      "responseTime": 8,
      "lastChecked": "2025-08-31T10:30:00Z"
    },
    {
      "name": "customer-service",
      "status": "degraded",
      "responseTime": 150,
      "lastChecked": "2025-08-31T10:30:00Z"
    }
  ]
}
```

#### Frontend Implementation
```typescript
interface ServiceHealthItem {
  name: string;
  status: 'healthy' | 'degraded' | 'unhealthy';
  responseTime: number;
  lastChecked: string;
}

interface ServicesHealthDto {
  status: 'healthy' | 'degraded' | 'unhealthy';
  services: ServiceHealthItem[];
}
```

---

### 4. **Backup Health**
**`GET /api/health/backup`**

Get database backup status and schedule information.

#### Success Response (200)
```json
{
  "status": "success",
  "lastBackup": {
    "timestamp": "2025-08-31T02:00:00Z",
    "duration": 1847,
    "size": "2.3GB",
    "type": "full"
  },
  "nextScheduled": "2025-09-01T02:00:00Z",
  "retentionDays": 30,
  "backupLocation": "local-storage"
}
```

#### Frontend Implementation
```typescript
interface BackupInfoDto {
  timestamp: string;
  duration: number; // seconds
  size: string;
  type: 'full' | 'incremental' | 'differential' | 'none';
}

interface BackupHealthDto {
  status: 'success' | 'failed' | 'running' | 'no-backup' | 'error';
  lastBackup: BackupInfoDto;
  nextScheduled: string;
  retentionDays: number;
  backupLocation: string;
}
```

---

### 5. **Storage Health**
**`GET /api/health/storage`**

Get disk space usage for different system components.

#### Success Response (200)
```json
{
  "database": {
    "used": "2.1GB",
    "total": "5.0GB",
    "percentage": 42.0,
    "status": "normal"
  },
  "files": {
    "used": "850MB",
    "total": "2.0GB",
    "percentage": 42.5,
    "status": "normal"
  },
  "logs": {
    "used": "125MB",
    "total": "500MB",
    "percentage": 25.0,
    "status": "normal"
  }
}
```

#### Frontend Implementation
```typescript
interface StorageComponentDto {
  used: string;
  total: string;
  percentage: number;
  status: 'normal' | 'warning' | 'critical';
}

interface StorageHealthDto {
  database: StorageComponentDto;
  files: StorageComponentDto;
  logs: StorageComponentDto;
}
```

---

### 6. **System Metrics**
**`GET /api/health/metrics`**

Get server performance metrics (CPU, Memory, Network).

#### Success Response (200)
```json
{
  "cpu": {
    "usage": 45.2,
    "cores": 8,
    "status": "normal"
  },
  "memory": {
    "used": "3.2GB",
    "total": "8.0GB",
    "percentage": 40.0,
    "status": "normal"
  },
  "network": {
    "inbound": "1.2MB/s",
    "outbound": "800KB/s",
    "status": "normal"
  }
}
```

#### Frontend Implementation
```typescript
interface CpuMetricsDto {
  usage: number;
  cores: number;
  status: 'normal' | 'warning' | 'critical';
}

interface MemoryMetricsDto {
  used: string;
  total: string;
  percentage: number;
  status: 'normal' | 'warning' | 'critical';
}

interface NetworkMetricsDto {
  inbound: string;
  outbound: string;
  status: 'normal' | 'high' | 'warning';
}

interface SystemMetricsDto {
  cpu: CpuMetricsDto;
  memory: MemoryMetricsDto;
  network: NetworkMetricsDto;
}
```

---

### 7. **Logs Health**
**`GET /api/health/logs`**

Get application error and warning summary.

#### Success Response (200)
```json
{
  "errors": {
    "last24h": 2,
    "last7d": 15,
    "status": "normal"
  },
  "warnings": {
    "last24h": 8,
    "last7d": 45,
    "status": "normal"
  },
  "logLevel": "info",
  "lastError": {
    "timestamp": "2025-08-31T09:15:00Z",
    "message": "Database connection timeout occurred",
    "level": "error"
  }
}
```

#### Frontend Implementation
```typescript
interface LogMetricsDto {
  last24h: number;
  last7d: number;
  status: 'normal' | 'medium' | 'high';
}

interface LastErrorDto {
  timestamp: string;
  message: string;
  level: string;
}

interface LogsHealthDto {
  errors: LogMetricsDto;
  warnings: LogMetricsDto;
  logLevel: string;
  lastError?: LastErrorDto;
}
```

---

### 8. **Security Health** 🔐
**`GET /api/health/security`**

Get security monitoring and certificate status. **Requires Admin role**.

#### Success Response (200)
```json
{
  "status": "secure",
  "lastSecurityScan": "2025-08-31T06:00:00Z",
  "certificates": {
    "status": "valid",
    "expiryDate": "2026-08-31T00:00:00Z",
    "daysUntilExpiry": 365
  },
  "authenticationFailures": 3,
  "activeTokens": 12
}
```

#### Frontend Implementation
```typescript
interface CertificatesDto {
  status: 'valid' | 'expiring' | 'expired';
  expiryDate: string;
  daysUntilExpiry: number;
}

interface SecurityHealthDto {
  status: 'secure' | 'warning' | 'critical';
  lastSecurityScan: string;
  certificates: CertificatesDto;
  authenticationFailures: number;
  activeTokens: number;
}
```

---

### 9. **Health Overview** 📊
**`GET /api/health/overview`**

Get comprehensive health status combining all metrics.

#### Success Response (200)
```json
{
  "general": {
    "status": "healthy",
    "timestamp": "2025-08-31T10:30:00Z",
    "version": "1.0.0",
    "uptime": 86400,
    "environment": "Production"
  },
  "database": {
    "status": "connected",
    "responseTime": 45
  },
  "services": {
    "status": "healthy",
    "healthyCount": 4,
    "totalCount": 5
  },
  "backup": {
    "status": "success",
    "lastBackup": {
      "timestamp": "2025-08-31T02:00:00Z",
      "duration": 1847,
      "size": "2.3GB",
      "type": "full"
    }
  },
  "storage": {
    "database": { "percentage": 42.0, "status": "normal" },
    "files": { "percentage": 42.5, "status": "normal" },
    "logs": { "percentage": 25.0, "status": "normal" }
  },
  "metrics": {
    "cpu": { "usage": 45.2, "status": "normal" },
    "memory": { "percentage": 40.0, "status": "normal" },
    "network": { "status": "normal" }
  }
}
```

---

### 10. **Health Ping** 🏓
**`GET /api/health/ping`**

Simple health check endpoint for load balancers. **No authentication required**.

#### Success Response (200)
```json
{
  "status": "healthy",
  "timestamp": "2025-08-31T10:30:00Z"
}
```

#### Error Response (503)
```json
{
  "status": "unhealthy",
  "timestamp": "2025-08-31T10:30:00Z"
}
```

#### Frontend Implementation
```typescript
// No auth required - perfect for monitoring
async function pingHealth(): Promise<{status: string, timestamp: string}> {
  const response = await fetch('/api/health/ping');
  return await response.json();
}
```

---

## 🎨 Frontend Integration Examples

### React Health Dashboard Component
```typescript
import React, { useEffect, useState } from 'react';

interface HealthStatus {
  system: SystemHealthDto;
  database: DatabaseHealthDto;
  storage: StorageHealthDto;
  metrics: SystemMetricsDto;
}

const HealthDashboard: React.FC = () => {
  const [health, setHealth] = useState<HealthStatus | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchHealthData = async () => {
      try {
        const [system, database, storage, metrics] = await Promise.all([
          healthApi.getSystemHealth(),
          healthApi.getDatabaseHealth(),
          healthApi.getStorageHealth(),
          healthApi.getSystemMetrics()
        ]);

        setHealth({ system, database, storage, metrics });
      } catch (error) {
        console.error('Failed to fetch health data:', error);
      } finally {
        setLoading(false);
      }
    };

    fetchHealthData();
    const interval = setInterval(fetchHealthData, 30000); // Refresh every 30 seconds

    return () => clearInterval(interval);
  }, []);

  if (loading) return <div>Loading health data...</div>;

  return (
    <div className="health-dashboard">
      <HealthStatusCard title="System" status={health?.system.status} />
      <HealthStatusCard title="Database" status={health?.database.status} />
      <StorageCard data={health?.storage} />
      <MetricsCard data={health?.metrics} />
    </div>
  );
};
```

### Angular Health Service
```typescript
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class HealthService {
  private baseUrl = '/api/health';

  constructor(private http: HttpClient) {}

  getSystemHealth(): Observable<SystemHealthDto> {
    return this.http.get<SystemHealthDto>(this.baseUrl);
  }

  getDatabaseHealth(): Observable<DatabaseHealthDto> {
    return this.http.get<DatabaseHealthDto>(`${this.baseUrl}/database`);
  }

  getHealthOverview(): Observable<HealthOverviewDto> {
    return this.http.get<HealthOverviewDto>(`${this.baseUrl}/overview`);
  }

  // Real-time health monitoring
  monitorHealth(): Observable<SystemHealthDto> {
    return new Observable(observer => {
      const interval = setInterval(async () => {
        try {
          const health = await this.getSystemHealth().toPromise();
          observer.next(health);
        } catch (error) {
          observer.error(error);
        }
      }, 10000); // Every 10 seconds

      return () => clearInterval(interval);
    });
  }
}
```

### Vue.js Health Composable
```typescript
import { ref, onMounted, onUnmounted } from 'vue';

export function useHealthMonitoring() {
  const healthData = ref<HealthOverviewDto | null>(null);
  const isLoading = ref(true);
  const error = ref<string | null>(null);
  
  let intervalId: number;

  const fetchHealth = async () => {
    try {
      const response = await fetch('/api/health/overview', {
        headers: {
          'Authorization': `Bearer ${getToken()}`,
          'Content-Type': 'application/json'
        }
      });

      if (!response.ok) {
        throw new Error(`HTTP ${response.status}`);
      }

      healthData.value = await response.json();
      error.value = null;
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Unknown error';
    } finally {
      isLoading.value = false;
    }
  };

  onMounted(() => {
    fetchHealth();
    intervalId = setInterval(fetchHealth, 30000);
  });

  onUnmounted(() => {
    if (intervalId) clearInterval(intervalId);
  });

  return {
    healthData: readonly(healthData),
    isLoading: readonly(isLoading),
    error: readonly(error),
    refetch: fetchHealth
  };
}
```

---

## 🚨 Error Handling

### Common HTTP Status Codes
- **200 OK**: Request successful, system healthy
- **401 Unauthorized**: Missing or invalid JWT token
- **403 Forbidden**: Insufficient permissions (for security endpoint)
- **500 Internal Server Error**: Server error occurred
- **503 Service Unavailable**: System is unhealthy/degraded

### Frontend Error Handling Pattern
```typescript
async function handleHealthRequest<T>(request: Promise<Response>): Promise<T> {
  try {
    const response = await request;
    
    if (response.status === 401) {
      // Redirect to login
      window.location.href = '/login';
      throw new Error('Authentication required');
    }
    
    if (response.status === 503) {
      // System is unhealthy but data is still valid
      return await response.json();
    }
    
    if (!response.ok) {
      throw new Error(`HTTP ${response.status}: ${response.statusText}`);
    }
    
    return await response.json();
  } catch (error) {
    console.error('Health API error:', error);
    throw error;
  }
}
```

---

## 📈 Best Practices for Frontend Integration

### 1. **Polling Frequency**
- **Overview/Dashboard**: Every 30-60 seconds
- **Real-time Monitoring**: Every 10-15 seconds
- **Background Checks**: Every 2-5 minutes

### 2. **Caching Strategy**
```typescript
class HealthCache {
  private cache = new Map<string, {data: any, expiry: number}>();
  
  get<T>(key: string): T | null {
    const item = this.cache.get(key);
    if (!item || Date.now() > item.expiry) {
      this.cache.delete(key);
      return null;
    }
    return item.data;
  }
  
  set<T>(key: string, data: T, ttlMs: number): void {
    this.cache.set(key, {
      data,
      expiry: Date.now() + ttlMs
    });
  }
}
```

### 3. **Status Indicators**
```css
.health-status {
  &.healthy { color: #28a745; }
  &.warning { color: #ffc107; }
  &.critical { color: #dc3545; }
  &.degraded { color: #fd7e14; }
}
```

### 4. **Progressive Enhancement**
- Always show basic status even if detailed metrics fail
- Gracefully handle network timeouts
- Provide manual refresh capability
- Cache last known good state

This documentation provides everything the frontend team needs to integrate with the Health Monitoring API effectively!
