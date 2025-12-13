# Status Summary - SMS API Application

This document provides a comprehensive overview of all status values used throughout the SMS API application.

## Table of Contents

1. [Business Domain Statuses](#business-domain-statuses)
2. [Health Monitoring Statuses](#health-monitoring-statuses)
3. [Backup Statuses](#backup-statuses)
4. [Service Statuses](#service-statuses)

---

## Business Domain Statuses

### 1. Item Status (Enum)

**Location:** `Enums/Status.cs`

**Type:** `ItemStatus` enum (Flags attribute)

| Value | Name      | Description                            |
| ----- | --------- | -------------------------------------- |
| 0     | Non       | No status / Not set                    |
| 1     | InStock   | Item is currently in stock (pawned)    |
| 2     | Redeemed  | Item has been redeemed by customer     |
| 3     | Defaulted | Item has been defaulted (not redeemed) |

**Usage:**

- Used in `Item` model (`Models/Item.cs`) - Default value: 1 (InStock)
- Set to `Redeemed` (2) when settlement invoice is processed
- Queried in `ItemService.cs` to count items with status 1 or 3

### 2. Invoice Status

**Location:** `Models/Invoice.cs`

**Type:** `int?` (nullable integer)

| Value | Description          |
| ----- | -------------------- |
| 1     | Active/Valid invoice |

**Usage:**

- Set to 1 when invoice is created in `BusinessLogic.cs`
- Used in invoice creation for all invoice types

### 3. Transaction Type (Enum)

**Location:** `Models/Transaction.cs`

**Type:** `TransactionType` enum

| Value | Name               | Description                   |
| ----- | ------------------ | ----------------------------- |
| 1     | LoanIssuance       | Initial loan/pawn transaction |
| 2     | InstallmentPayment | Payment of loan installment   |
| 3     | InterestPayment    | Interest-only payment         |
| 4     | LateFeePayment     | Late fee payment              |
| 5     | LoanClosure        | Final settlement/closure      |

**Usage:**

- Used in `Transaction` model to categorize transaction types
- Referenced in `BusinessLogic.cs` for different invoice processing flows

### 4. Invoice Type (Enum)

**Location:** `Models/Invoice.cs`

**Type:** `InvoiceType` enum

| Value | Name                      | Description                     |
| ----- | ------------------------- | ------------------------------- |
| 1     | InitialPawnInvoice        | Invoice for initial pawn/loan   |
| 2     | InstallmentPaymentInvoice | Invoice for installment payment |
| 3     | SettlementInvoice         | Invoice for loan settlement     |

**Usage:**

- Used in `Invoice` model as `InvoiceTypeId`
- Drives business logic flow in `BusinessLogic.ProcessInvoice()`

### 5. Loan Settlement Status

**Location:** `Models/Loan.cs`

**Type:** `Boolean`

| Value | Description                |
| ----- | -------------------------- |
| true  | Loan is settled/closed     |
| false | Loan is active/outstanding |

**Usage:**

- `IsSettled` property in `Loan` model
- Set to true when settlement invoice is processed

---

## Health Monitoring Statuses

### 1. System Health Status

**Location:** `Services/HealthService.cs`

**Type:** String

| Status      | Description                    | Condition                 |
| ----------- | ------------------------------ | ------------------------- |
| "healthy"   | System is functioning normally | All checks pass           |
| "unhealthy" | System has issues              | Database connection fails |

**Usage:**

- System-level health indicator
- Logged in `SystemHealthLog` table

### 2. Database Health Status

**Location:** `Services/HealthService.cs`

**Type:** String

| Status         | Description                | Condition             |
| -------------- | -------------------------- | --------------------- |
| "connected"    | Database is accessible     | Connection successful |
| "disconnected" | Database is not accessible | Connection failed     |

**Usage:**

- Database connectivity status
- Checked in health monitoring and alerting services

### 3. Services Health Status

**Location:** `Services/HealthService.cs`

**Type:** String

| Status        | Description                 | Condition                     |
| ------------- | --------------------------- | ----------------------------- |
| "operational" | Service is running normally | Service responds successfully |
| "degraded"    | Service has issues          | One or more services are down |
| "down"        | Service is not responding   | Service check fails           |

**Usage:**

- Individual service health status
- Overall services health aggregation

### 4. Storage Status

**Location:** `Services/HealthService.cs`

**Type:** String

| Status     | Description                 | Condition                               |
| ---------- | --------------------------- | --------------------------------------- |
| "normal"   | Storage usage is acceptable | Below warning threshold                 |
| "warning"  | Storage usage is high       | Above warning threshold (configurable)  |
| "critical" | Storage usage is critical   | Above critical threshold (configurable) |

**Usage:**

- Applied to Database, Files, and Logs storage
- Triggers alerts when critical

### 5. Metrics Status (CPU/Memory)

**Location:** `Services/HealthService.cs`

**Type:** String

| Status     | Description                  | Condition                |
| ---------- | ---------------------------- | ------------------------ |
| "normal"   | Resource usage is acceptable | Below warning threshold  |
| "warning"  | Resource usage is high       | Above warning threshold  |
| "critical" | Resource usage is critical   | Above critical threshold |

**Usage:**

- CPU and Memory usage monitoring
- Configurable thresholds in `appsettings.json`

### 6. Network Status

**Location:** `Services/HealthService.cs`

**Type:** String

| Status    | Description                  | Condition               |
| --------- | ---------------------------- | ----------------------- |
| "normal"  | Network traffic is normal    | Below high threshold    |
| "high"    | Network traffic is elevated  | Above normal threshold  |
| "warning" | Network traffic is very high | Above warning threshold |
| "unknown" | Unable to determine status   | Error in measurement    |

**Usage:**

- Network traffic monitoring
- Inbound/outbound traffic status

### 7. Logs Health Status

**Location:** `Services/HealthService.cs`

**Type:** String

| Status   | Description              | Condition            |
| -------- | ------------------------ | -------------------- |
| "low"    | Few errors/warnings      | Below warning count  |
| "medium" | Moderate errors/warnings | Above warning count  |
| "high"   | Many errors/warnings     | Above critical count |

**Usage:**

- Error and warning log monitoring
- Separate status for errors and warnings

### 8. Security Status

**Location:** `Services/HealthService.cs`

**Type:** String

| Status   | Description          | Condition                   |
| -------- | -------------------- | --------------------------- |
| "secure" | No security issues   | All security checks pass    |
| "valid"  | Certificate is valid | SSL certificate not expired |

**Usage:**

- Overall security health
- SSL certificate validation

---

## Backup Statuses

### 1. Backup History Status

**Location:** `Models/HealthModels.cs` (BackupHistory table)

**Type:** String (MaxLength: 50)

| Status      | Description                    | When Set                         |
| ----------- | ------------------------------ | -------------------------------- |
| "running"   | Backup is in progress          | Backup starts                    |
| "success"   | Backup completed successfully  | Backup completes without errors  |
| "failed"    | Backup failed                  | Backup encounters an error       |
| "no-backup" | No backup exists               | No backup records found          |
| "error"     | Error retrieving backup status | Exception in backup health check |

**Usage:**

- Stored in `BackupHistory` table
- Used in backup health monitoring
- Triggers alerts on failure

---

## Service Statuses

### 1. Service Health Status

**Location:** `Models/HealthModels.cs` (ServiceHealthStatus table)

**Type:** String (MaxLength: 50)

| Status        | Description                     |
| ------------- | ------------------------------- |
| "operational" | Service is functioning normally |
| "down"        | Service is not responding       |

**Usage:**

- Stored in `ServiceHealthStatus` table
- Monitored by `HealthAlertingService`

### 2. System Health Log Status

**Location:** `Models/HealthModels.cs` (SystemHealthLog table)

**Type:** String (MaxLength: 50)

**Description:** Generic status field that stores various status values from different components:

- "healthy" / "unhealthy" (system)
- "connected" / "disconnected" (database)
- "operational" / "degraded" / "down" (services)
- "checked" (storage)
- "collected" (metrics)
- "normal" / "warning" / "critical" (various metrics)
- "secure" (security)
- And many others from different health checks

**Usage:**

- Central logging table for all health status changes
- Includes component name, status, metrics JSON, and timestamp
- Used for historical health tracking and analysis

---

## Status Configuration

### Configurable Thresholds

**Location:** `appsettings.json` - `HealthMonitoring` section

#### Storage Thresholds

- **WarningPercentage**: Default 70%
- **CriticalPercentage**: Default 85%

#### Metrics Thresholds

- **CPU Warning**: Default 70%
- **CPU Critical**: Default 85%
- **Memory Warning**: Default 75%
- **Memory Critical**: Default 90%

#### Logs Thresholds

- **Errors Warning Count**: Default 10
- **Errors Critical Count**: Default 50
- **Warnings Normal Count**: Default 20
- **Warnings High Count**: Default 100

---

## Summary Statistics

### Total Status Types by Category:

1. **Business Domain**: 4 enum types (ItemStatus, TransactionType, InvoiceType, + boolean IsSettled)
2. **Health Monitoring**: 8 status categories (System, Database, Services, Storage, Metrics, Network, Logs, Security)
3. **Backup**: 5 distinct status values
4. **Service Health**: 2 primary status values

### Total Unique Status Values: ~30+ distinct status strings

---

## Notes

1. **Business Statuses** are primarily integer-based enums for database efficiency
2. **Health Monitoring Statuses** are string-based for flexibility and readability
3. **Status values are case-sensitive** in the codebase
4. **Thresholds are configurable** via `appsettings.json` for health monitoring
5. **Status logging** is centralized in `SystemHealthLog` table for all health-related statuses
6. **Backup statuses** are persisted in `BackupHistory` table for audit trail

---

**Document Generated:** 2025-11-24  
**Application:** SMS API (Shop Management System)  
**Version:** Current as of latest codebase analysis
