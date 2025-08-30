using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMS.Interfaces;
using SMS.Models.DTO;

namespace SMS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Require authentication for all health endpoints
    public class HealthController : ControllerBase
    {
        private readonly IHealthService _healthService;
        private readonly ILogger<HealthController> _logger;

        public HealthController(IHealthService healthService, ILogger<HealthController> logger)
        {
            _healthService = healthService;
            _logger = logger;
        }

        /// <summary>
        /// Get overall system health status and basic metrics
        /// </summary>
        /// <returns>System health information</returns>
        [HttpGet]
        [ProducesResponseType(typeof(SystemHealthDto), 200)]
        [ProducesResponseType(503)]
        public async Task<ActionResult<SystemHealthDto>> GetSystemHealth()
        {
            try
            {
                var health = await _healthService.GetSystemHealthAsync();
                
                // Return 503 Service Unavailable if system is unhealthy
                if (health.Status == "unhealthy")
                {
                    return StatusCode(503, health);
                }

                return Ok(health);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get system health");
                return StatusCode(503, new SystemHealthDto 
                { 
                    Status = "unhealthy", 
                    Timestamp = DateTime.UtcNow 
                });
            }
        }

        /// <summary>
        /// Get database connection status and performance metrics
        /// </summary>
        /// <returns>Database health information</returns>
        [HttpGet("database")]
        [ProducesResponseType(typeof(DatabaseHealthDto), 200)]
        [ProducesResponseType(503)]
        public async Task<ActionResult<DatabaseHealthDto>> GetDatabaseHealth()
        {
            try
            {
                var health = await _healthService.GetDatabaseHealthAsync();
                
                if (health.Status != "connected")
                {
                    return StatusCode(503, health);
                }

                return Ok(health);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get database health");
                return StatusCode(503, new DatabaseHealthDto 
                { 
                    Status = "error", 
                    LastChecked = DateTime.UtcNow 
                });
            }
        }

        /// <summary>
        /// Get health status of individual services
        /// </summary>
        /// <returns>Services health information</returns>
        [HttpGet("services")]
        [ProducesResponseType(typeof(ServicesHealthDto), 200)]
        [ProducesResponseType(503)]
        public async Task<ActionResult<ServicesHealthDto>> GetServicesHealth()
        {
            try
            {
                var health = await _healthService.GetServicesHealthAsync();
                
                if (health.Status == "degraded")
                {
                    return StatusCode(503, health);
                }

                return Ok(health);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get services health");
                return StatusCode(503, new ServicesHealthDto 
                { 
                    Status = "error" 
                });
            }
        }

        /// <summary>
        /// Get database backup status and schedule information
        /// </summary>
        /// <returns>Backup health information</returns>
        [HttpGet("backup")]
        [ProducesResponseType(typeof(BackupHealthDto), 200)]
        [ProducesResponseType(503)]
        public async Task<ActionResult<BackupHealthDto>> GetBackupHealth()
        {
            try
            {
                var health = await _healthService.GetBackupHealthAsync();
                
                if (health.Status == "failed" || health.Status == "error")
                {
                    return StatusCode(503, health);
                }

                return Ok(health);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get backup health");
                return StatusCode(503, new BackupHealthDto 
                { 
                    Status = "error" 
                });
            }
        }

        /// <summary>
        /// Get disk space usage for different components
        /// </summary>
        /// <returns>Storage usage information</returns>
        [HttpGet("storage")]
        [ProducesResponseType(typeof(StorageHealthDto), 200)]
        [ProducesResponseType(503)]
        public async Task<ActionResult<StorageHealthDto>> GetStorageHealth()
        {
            try
            {
                var health = await _healthService.GetStorageHealthAsync();
                
                // Check if any storage component is critical
                var hasCriticalStorage = health.Database.Status == "critical" ||
                                       health.Files.Status == "critical" ||
                                       health.Logs.Status == "critical";

                if (hasCriticalStorage)
                {
                    return StatusCode(503, health);
                }

                return Ok(health);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get storage health");
                return StatusCode(500, "Failed to retrieve storage health information");
            }
        }

        /// <summary>
        /// Get server performance metrics (CPU, Memory, Network)
        /// </summary>
        /// <returns>System performance metrics</returns>
        [HttpGet("metrics")]
        [ProducesResponseType(typeof(SystemMetricsDto), 200)]
        [ProducesResponseType(503)]
        public async Task<ActionResult<SystemMetricsDto>> GetSystemMetrics()
        {
            try
            {
                var metrics = await _healthService.GetSystemMetricsAsync();
                
                // Check if any metric is critical
                var hasCriticalMetrics = metrics.Cpu.Status == "critical" ||
                                       metrics.Memory.Status == "critical" ||
                                       metrics.Network.Status == "critical";

                if (hasCriticalMetrics)
                {
                    return StatusCode(503, metrics);
                }

                return Ok(metrics);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get system metrics");
                return StatusCode(500, "Failed to retrieve system metrics");
            }
        }

        /// <summary>
        /// Get application error and warning summary
        /// </summary>
        /// <returns>Application logs health information</returns>
        [HttpGet("logs")]
        [ProducesResponseType(typeof(LogsHealthDto), 200)]
        [ProducesResponseType(503)]
        public async Task<ActionResult<LogsHealthDto>> GetLogsHealth()
        {
            try
            {
                var health = await _healthService.GetLogsHealthAsync();
                
                if (health.Errors.Status == "high")
                {
                    return StatusCode(503, health);
                }

                return Ok(health);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get logs health");
                return StatusCode(500, "Failed to retrieve logs health information");
            }
        }

        /// <summary>
        /// Get security monitoring and certificate status
        /// </summary>
        /// <returns>Security health information</returns>
        [HttpGet("security")]
        [ProducesResponseType(typeof(SecurityHealthDto), 200)]
        [ProducesResponseType(503)]
        [Authorize(Policy = "AdminPolicy")] // Only admins can access security info
        public async Task<ActionResult<SecurityHealthDto>> GetSecurityHealth()
        {
            try
            {
                var health = await _healthService.GetSecurityHealthAsync();
                
                if (health.Status == "critical")
                {
                    return StatusCode(503, health);
                }

                return Ok(health);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get security health");
                return StatusCode(500, "Failed to retrieve security health information");
            }
        }

        /// <summary>
        /// Get comprehensive health status combining all metrics
        /// </summary>
        /// <returns>Complete health overview</returns>
        [HttpGet("overview")]
        [ProducesResponseType(typeof(HealthOverviewDto), 200)]
        [ProducesResponseType(503)]
        public async Task<ActionResult<HealthOverviewDto>> GetHealthOverview()
        {
            try
            {
                var overview = await _healthService.GetHealthOverviewAsync();
                
                // Check if overall system is unhealthy
                if (overview.General.Status == "unhealthy" || 
                    overview.Database.Status != "connected")
                {
                    return StatusCode(503, overview);
                }

                return Ok(overview);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get health overview");
                return StatusCode(503, new HealthOverviewDto 
                { 
                    General = new SystemHealthDto 
                    { 
                        Status = "unhealthy", 
                        Timestamp = DateTime.UtcNow 
                    } 
                });
            }
        }

        /// <summary>
        /// Health check endpoint for load balancers (simple endpoint without auth)
        /// </summary>
        /// <returns>Simple health status</returns>
        [HttpGet("ping")]
        [AllowAnonymous] // Allow anonymous access for load balancer health checks
        [ProducesResponseType(200)]
        [ProducesResponseType(503)]
        public async Task<IActionResult> Ping()
        {
            try
            {
                var health = await _healthService.GetSystemHealthAsync();
                
                if (health.Status == "unhealthy")
                {
                    return StatusCode(503, new { status = "unhealthy", timestamp = DateTime.UtcNow });
                }

                return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
            }
            catch
            {
                return StatusCode(503, new { status = "unhealthy", timestamp = DateTime.UtcNow });
            }
        }
    }
}
