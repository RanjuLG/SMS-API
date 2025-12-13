# SMS API Environment Variables Verification Script
# This script checks if all required environment variables are properly set

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet('User', 'System', 'Process')]
    [string]$Scope = 'User'
)

function Test-EnvironmentVariable {
    param(
        [string]$Name,
        [string]$Scope
    )
    
    $value = [System.Environment]::GetEnvironmentVariable($Name, [System.EnvironmentVariableTarget]::$Scope)
    
    if ([string]::IsNullOrEmpty($value)) {
        return @{
            Name = $Name
            Status = "MISSING"
            Value = ""
            Color = "Red"
        }
    } else {
        # Mask sensitive values
        $displayValue = if ($Name -like "*Password*" -or $Name -like "*Key*" -or $Name -like "*ConnectionString*") {
            "[SET - $(($value.Length)) characters]"
        } else {
            $value
        }
        
        return @{
            Name = $Name
            Status = "OK"
            Value = $displayValue
            Color = "Green"
        }
    }
}

Write-Host ""
Write-Host "==================================================================" -ForegroundColor Cyan
Write-Host "SMS API Environment Variables Verification" -ForegroundColor Cyan
Write-Host "==================================================================" -ForegroundColor Cyan
Write-Host "Scope: $Scope" -ForegroundColor Yellow
Write-Host ""

# List of required environment variables
$requiredVars = @(
    "ASPNETCORE_ENVIRONMENT",
    "SMS_ConnectionStrings__MasterDatabase",
    "SMS_Jwt__Key",
    "SMS_Jwt__Issuer",
    "SMS_Jwt__Audience",
    "SMS_FileSettings__NicUploadFolderPath"
)

$results = @()
$missingCount = 0

foreach ($varName in $requiredVars) {
    $result = Test-EnvironmentVariable -Name $varName -Scope $Scope
    $results += $result
    
    if ($result.Status -eq "MISSING") {
        $missingCount++
    }
}

# Display results
Write-Host "Variable Status:" -ForegroundColor Yellow
Write-Host ""

foreach ($result in $results) {
    $statusSymbol = if ($result.Status -eq "OK") { "?" } else { "?" }
    Write-Host "  $statusSymbol $($result.Name)" -ForegroundColor $result.Color
    if ($result.Status -eq "OK") {
        Write-Host "    Value: $($result.Value)" -ForegroundColor Gray
    } else {
        Write-Host "    Value: NOT SET" -ForegroundColor Red
    }
    Write-Host ""
}

Write-Host "==================================================================" -ForegroundColor Cyan

if ($missingCount -eq 0) {
    Write-Host "? All required environment variables are set!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Next steps:" -ForegroundColor Yellow
    Write-Host "1. Restart your terminal/IDE if you just set these variables"
    Write-Host "2. Start the SMS API application"
    Write-Host "3. Check the logs for: 'Database configuration: Configuration loaded'"
    Write-Host "4. Test the health endpoint: GET /api/health/database"
} else {
    Write-Host "? $missingCount variable(s) missing!" -ForegroundColor Red
    Write-Host ""
    Write-Host "To set missing variables, run:" -ForegroundColor Yellow
    Write-Host "  .\Setup-Environment.ps1 -Scope $Scope -ConnectionString ""..."" -JwtKey ""..."""
    Write-Host ""
    Write-Host "Or set them manually using:" -ForegroundColor Yellow
    Write-Host "  setx VARIABLE_NAME ""value""" -NoNewline
    if ($Scope -eq "System") {
        Write-Host " /M" -ForegroundColor Yellow
    } else {
        Write-Host ""
    }
}

Write-Host "==================================================================" -ForegroundColor Cyan
Write-Host ""

# Additional checks
Write-Host "Additional Checks:" -ForegroundColor Yellow
Write-Host ""

# Check if upload path exists (if set)
$uploadPath = [System.Environment]::GetEnvironmentVariable("SMS_FileSettings__NicUploadFolderPath", [System.EnvironmentVariableTarget]::$Scope)
if (![string]::IsNullOrEmpty($uploadPath)) {
    if (Test-Path $uploadPath) {
        Write-Host "  ? Upload directory exists: $uploadPath" -ForegroundColor Green
    } else {
        Write-Host "  ? Upload directory does not exist: $uploadPath" -ForegroundColor Red
        Write-Host "    Create it with: New-Item -ItemType Directory -Path ""$uploadPath""" -ForegroundColor Yellow
    }
} else {
    Write-Host "  - Upload directory path not set" -ForegroundColor Gray
}

Write-Host ""

# Check JWT key length
$jwtKey = [System.Environment]::GetEnvironmentVariable("SMS_Jwt__Key", [System.EnvironmentVariableTarget]::$Scope)
if (![string]::IsNullOrEmpty($jwtKey)) {
    try {
        $keyBytes = [Convert]::FromBase64String($jwtKey)
        $keyLength = $keyBytes.Length * 8
        
        if ($keyLength -ge 256) {
            Write-Host "  ? JWT Key length: $keyLength bits (Secure)" -ForegroundColor Green
        } else {
            Write-Host "  ? JWT Key length: $keyLength bits (Too short, should be at least 256 bits)" -ForegroundColor Red
        }
    } catch {
        Write-Host "  ? JWT Key is not valid Base64" -ForegroundColor Red
    }
} else {
    Write-Host "  - JWT Key not set" -ForegroundColor Gray
}

Write-Host ""
Write-Host "==================================================================" -ForegroundColor Cyan
Write-Host ""

exit $missingCount
