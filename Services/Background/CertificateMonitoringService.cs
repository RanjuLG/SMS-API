using System.Security.Cryptography.X509Certificates;
using SMS.Models.Configuration;
using Microsoft.Extensions.Options;

namespace SMS.Services.Background
{
    public class CertificateMonitoringService : BackgroundService
    {
        private readonly ILogger<CertificateMonitoringService> _logger;
        private readonly HealthMonitoringConfiguration _config;
        private readonly TimeSpan _checkInterval = TimeSpan.FromHours(12); // Check twice daily

        public CertificateMonitoringService(
            ILogger<CertificateMonitoringService> logger,
            IOptions<HealthMonitoringConfiguration> config)
        {
            _logger = logger;
            _config = config.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Certificate Monitoring Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckCertificatesAsync();
                    await Task.Delay(_checkInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in certificate monitoring service");
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                }
            }

            _logger.LogInformation("Certificate Monitoring Service stopped");
        }

        private async Task CheckCertificatesAsync()
        {
            try
            {
                _logger.LogDebug("Checking SSL certificates...");

                var certificates = new List<CertificateInfo>();

                // Check certificates from different sources
                certificates.AddRange(await GetCertificatesFromStoreAsync(StoreName.My, StoreLocation.LocalMachine));
                certificates.AddRange(await GetCertificatesFromStoreAsync(StoreName.My, StoreLocation.CurrentUser));
                certificates.AddRange(await GetApplicationCertificatesAsync());

                foreach (var certInfo in certificates)
                {
                    await CheckCertificateExpiryAsync(certInfo);
                }

                _logger.LogDebug("Certificate check completed. Checked {Count} certificates", certificates.Count);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to check certificates");
            }
        }

        private async Task<List<CertificateInfo>> GetCertificatesFromStoreAsync(StoreName storeName, StoreLocation storeLocation)
        {
            var certificates = new List<CertificateInfo>();

            try
            {
                using var store = new X509Store(storeName, storeLocation);
                store.Open(OpenFlags.ReadOnly);

                foreach (X509Certificate2 cert in store.Certificates)
                {
                    // Only check certificates that are currently valid and used for server authentication
                    if (cert.HasPrivateKey && 
                        cert.NotBefore <= DateTime.Now && 
                        cert.NotAfter > DateTime.Now &&
                        HasServerAuthUsage(cert))
                    {
                        certificates.Add(new CertificateInfo
                        {
                            Subject = cert.Subject,
                            Issuer = cert.Issuer,
                            Thumbprint = cert.Thumbprint,
                            NotBefore = cert.NotBefore,
                            NotAfter = cert.NotAfter,
                            Store = $"{storeLocation}/{storeName}"
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to read certificates from store: {StoreName}/{StoreLocation}", 
                    storeName, storeLocation);
            }

            return certificates;
        }

        private async Task<List<CertificateInfo>> GetApplicationCertificatesAsync()
        {
            var certificates = new List<CertificateInfo>();

            try
            {
                // Check for certificate files in common locations
                var certPaths = new[]
                {
                    Path.Combine(Directory.GetCurrentDirectory(), "certificates"),
                    Path.Combine(Directory.GetCurrentDirectory(), "certs"),
                    Path.Combine(Directory.GetCurrentDirectory(), "ssl")
                };

                foreach (var certPath in certPaths)
                {
                    if (Directory.Exists(certPath))
                    {
                        var certFiles = Directory.GetFiles(certPath, "*.cer")
                            .Concat(Directory.GetFiles(certPath, "*.crt"))
                            .Concat(Directory.GetFiles(certPath, "*.pem"))
                            .Concat(Directory.GetFiles(certPath, "*.pfx"));

                        foreach (var certFile in certFiles)
                        {
                            try
                            {
                                var cert = new X509Certificate2(certFile);
                                certificates.Add(new CertificateInfo
                                {
                                    Subject = cert.Subject,
                                    Issuer = cert.Issuer,
                                    Thumbprint = cert.Thumbprint,
                                    NotBefore = cert.NotBefore,
                                    NotAfter = cert.NotAfter,
                                    Store = $"File: {Path.GetFileName(certFile)}"
                                });
                            }
                            catch (Exception ex)
                            {
                                _logger.LogDebug(ex, "Could not read certificate file: {CertFile}", certFile);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to check application certificate files");
            }

            return certificates;
        }

        private bool HasServerAuthUsage(X509Certificate2 certificate)
        {
            try
            {
                foreach (X509Extension extension in certificate.Extensions)
                {
                    if (extension is X509EnhancedKeyUsageExtension ekuExtension)
                    {
                        foreach (var oid in ekuExtension.EnhancedKeyUsages)
                        {
                            // Server Authentication OID
                            if (oid.Value == "1.3.6.1.5.5.7.3.1")
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            catch
            {
                // If we can't determine usage, assume it might be a server cert
                return true;
            }

            return false;
        }

        private async Task CheckCertificateExpiryAsync(CertificateInfo certInfo)
        {
            try
            {
                var now = DateTime.Now;
                var daysUntilExpiry = (certInfo.NotAfter - now).Days;

                var logMessage = "Certificate {Subject} from {Store} expires in {Days} days (on {ExpiryDate})";
                var logArgs = new object[] { certInfo.Subject, certInfo.Store, daysUntilExpiry, certInfo.NotAfter.ToString("yyyy-MM-dd") };

                if (daysUntilExpiry <= 0)
                {
                    _logger.LogCritical("🔴 EXPIRED: " + logMessage, logArgs);
                    await LogCertificateAlertAsync(certInfo, "EXPIRED", $"Certificate has expired on {certInfo.NotAfter:yyyy-MM-dd}");
                }
                else if (daysUntilExpiry <= 7)
                {
                    _logger.LogCritical("🚨 CRITICAL: " + logMessage, logArgs);
                    await LogCertificateAlertAsync(certInfo, "CRITICAL", $"Certificate expires in {daysUntilExpiry} days");
                }
                else if (daysUntilExpiry <= 30)
                {
                    _logger.LogWarning("⚠️  WARNING: " + logMessage, logArgs);
                    await LogCertificateAlertAsync(certInfo, "WARNING", $"Certificate expires in {daysUntilExpiry} days");
                }
                else if (daysUntilExpiry <= 90)
                {
                    _logger.LogInformation("ℹ️  INFO: " + logMessage, logArgs);
                }
                else
                {
                    _logger.LogDebug("✅ OK: " + logMessage, logArgs);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to check certificate expiry for: {Subject}", certInfo.Subject);
            }
        }

        private async Task LogCertificateAlertAsync(CertificateInfo certInfo, string level, string message)
        {
            try
            {
                var alertMessage = $"""
                    ===============================================
                    🔐 CERTIFICATE ALERT - {level}
                    ===============================================
                    Subject: {certInfo.Subject}
                    Issuer: {certInfo.Issuer}
                    Store: {certInfo.Store}
                    Thumbprint: {certInfo.Thumbprint}
                    Valid From: {certInfo.NotBefore:yyyy-MM-dd HH:mm:ss}
                    Valid Until: {certInfo.NotAfter:yyyy-MM-dd HH:mm:ss}
                    Message: {message}
                    Time: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC
                    ===============================================
                    """;

                Console.WriteLine(alertMessage);

                // Write to certificate alerts log
                var alertsLogPath = Path.Combine("logs", "certificate-alerts.log");
                Directory.CreateDirectory(Path.GetDirectoryName(alertsLogPath));
                await File.AppendAllTextAsync(alertsLogPath, alertMessage + Environment.NewLine);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to log certificate alert");
            }
        }
    }

    public class CertificateInfo
    {
        public string Subject { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Thumbprint { get; set; } = string.Empty;
        public DateTime NotBefore { get; set; }
        public DateTime NotAfter { get; set; }
        public string Store { get; set; } = string.Empty;
    }
}
