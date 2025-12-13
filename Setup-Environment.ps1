# SMS API Environment Setup Script for Windows
# This script helps set up environment variables for the SMS API
# Run with administrator privileges to set system-wide variables

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet('User', 'System')]
    [string]$Scope = 'User',
    
    [Parameter(Mandatory=$false)]
    [string]$Environment = 'Production',
    
    [Parameter(Mandatory=$true)]
    [string]$ConnectionString,
    
    [Parameter(Mandatory=$true)]
    [string]$JwtKey,
    
    [Parameter(Mandatory=$false)]
    [string]$FileUploadPath = 'C:\inetpub\wwwroot\SMS_GUI\brower\uploads\nic'
)

# Check if running as administrator when System scope is selected
if ($Scope -eq 'System') {
    $currentPrincipal = New-Object Security.Principal.WindowsPrincipal([Security.Principal.WindowsIdentity]::GetCurrent())
    $isAdmin = $currentPrincipal.IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
    
    if (-not $isAdmin) {
        Write-Host "ERROR: System scope requires administrator privileges." -ForegroundColor Red
        Write-Host "Please run PowerShell as Administrator and try again." -ForegroundColor Yellow
        exit 1
    }
}

Write-Host "==================================================================" -ForegroundColor Cyan
Write-Host "SMS API Environment Setup" -ForegroundColor Cyan
Write-Host "==================================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Configuration:" -ForegroundColor Yellow
Write-Host "  Scope: $Scope"
Write-Host "  Environment: $Environment"
Write-Host "  Connection String: $(if($ConnectionString.Length -gt 50) { $ConnectionString.Substring(0,50) + '...' } else { $ConnectionString })"
Write-Host "  JWT Key: $(if($JwtKey.Length -gt 20) { $JwtKey.Substring(0,20) + '...' } else { $JwtKey })"
Write-Host "  Upload Path: $FileUploadPath"
Write-Host ""

# Confirm before proceeding
$confirm = Read-Host "Do you want to proceed with setting these environment variables? (Y/N)"
if ($confirm -ne 'Y' -and $confirm -ne 'y') {
    Write-Host "Operation cancelled." -ForegroundColor Yellow
    exit 0
}

Write-Host ""
Write-Host "Setting environment variables..." -ForegroundColor Green

try {
    # Set ASPNETCORE_ENVIRONMENT
    [System.Environment]::SetEnvironmentVariable(
        'ASPNETCORE_ENVIRONMENT', 
        $Environment, 
        [System.EnvironmentVariableTarget]::$Scope
    )
    Write-Host "? ASPNETCORE_ENVIRONMENT set to: $Environment" -ForegroundColor Green
    
    # Set Connection String
    [System.Environment]::SetEnvironmentVariable(
        'SMS_ConnectionStrings__MasterDatabase', 
        $ConnectionString, 
        [System.EnvironmentVariableTarget]::$Scope
    )
    Write-Host "? SMS_ConnectionStrings__MasterDatabase set" -ForegroundColor Green
    
    # Set JWT Key
    [System.Environment]::SetEnvironmentVariable(
        'SMS_Jwt__Key', 
        $JwtKey, 
        [System.EnvironmentVariableTarget]::$Scope
    )
    Write-Host "? SMS_Jwt__Key set" -ForegroundColor Green
    
    # Set JWT Issuer
    [System.Environment]::SetEnvironmentVariable(
        'SMS_Jwt__Issuer', 
        'SMS', 
        [System.EnvironmentVariableTarget]::$Scope
    )
    Write-Host "? SMS_Jwt__Issuer set" -ForegroundColor Green
    
    # Set JWT Audience
    [System.Environment]::SetEnvironmentVariable(
        'SMS_Jwt__Audience', 
        'SMS', 
        [System.EnvironmentVariableTarget]::$Scope
    )
    Write-Host "? SMS_Jwt__Audience set" -ForegroundColor Green
    
    # Set File Upload Path
    [System.Environment]::SetEnvironmentVariable(
        'SMS_FileSettings__NicUploadFolderPath', 
        $FileUploadPath, 
        [System.EnvironmentVariableTarget]::$Scope
    )
    Write-Host "? SMS_FileSettings__NicUploadFolderPath set" -ForegroundColor Green
    
    Write-Host ""
    Write-Host "==================================================================" -ForegroundColor Cyan
    Write-Host "Environment variables set successfully!" -ForegroundColor Green
    Write-Host "==================================================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "IMPORTANT: Next steps:" -ForegroundColor Yellow
    Write-Host "1. Close and reopen your terminal/IDE to load the new variables"
    Write-Host "2. If using IIS, restart the application pool or run: iisreset"
    Write-Host "3. If using Visual Studio, restart Visual Studio"
    Write-Host "4. Start the SMS API application"
    Write-Host "5. Check the logs to verify configuration loaded correctly"
    Write-Host ""
    
} catch {
    Write-Host ""
    Write-Host "ERROR: Failed to set environment variables" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    exit 1
}

# Display current values for verification (without showing sensitive data)
Write-Host "Verification (current session values):" -ForegroundColor Cyan
Write-Host "  ASPNETCORE_ENVIRONMENT: $([System.Environment]::GetEnvironmentVariable('ASPNETCORE_ENVIRONMENT', [System.EnvironmentVariableTarget]::$Scope))"
Write-Host "  SMS_ConnectionStrings__MasterDatabase: [SET]"
Write-Host "  SMS_Jwt__Key: [SET]"
Write-Host "  SMS_Jwt__Issuer: $([System.Environment]::GetEnvironmentVariable('SMS_Jwt__Issuer', [System.EnvironmentVariableTarget]::$Scope))"
Write-Host "  SMS_Jwt__Audience: $([System.Environment]::GetEnvironmentVariable('SMS_Jwt__Audience', [System.EnvironmentVariableTarget]::$Scope))"
Write-Host "  SMS_FileSettings__NicUploadFolderPath: $([System.Environment]::GetEnvironmentVariable('SMS_FileSettings__NicUploadFolderPath', [System.EnvironmentVariableTarget]::$Scope))"
Write-Host ""

# Example usage
Write-Host "==================================================================" -ForegroundColor Cyan
Write-Host "Example Usage:" -ForegroundColor Cyan
Write-Host "==================================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "# For User-level variables (no admin required):" -ForegroundColor Gray
Write-Host '.\Setup-Environment.ps1 -Scope User -ConnectionString "Data Source=localhost;..." -JwtKey "YourBase64Key"' -ForegroundColor Gray
Write-Host ""
Write-Host "# For System-level variables (requires admin):" -ForegroundColor Gray
Write-Host '.\Setup-Environment.ps1 -Scope System -ConnectionString "Data Source=localhost;..." -JwtKey "YourBase64Key"' -ForegroundColor Gray
Write-Host ""
