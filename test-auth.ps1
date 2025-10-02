# Test authentication script
try {
    # Test login
    Write-Host "Testing login..." -ForegroundColor Green
    $loginBody = @{
        username = "superadmin"
        password = "SuperAdmin@123"
    } | ConvertTo-Json
    
    $loginResponse = Invoke-RestMethod -Uri "http://localhost:7218/api/account/login" -Method POST -ContentType "application/json" -Body $loginBody
    Write-Host "Login response:" -ForegroundColor Yellow
    $loginResponse | ConvertTo-Json -Depth 3
    
    if ($loginResponse.token) {
        Write-Host "`nToken received! Length: $($loginResponse.token.Length)" -ForegroundColor Green
        
        # Test protected endpoint
        Write-Host "`nTesting protected endpoint..." -ForegroundColor Green
        $headers = @{
            "Authorization" = "Bearer $($loginResponse.token)"
        }
        
        $protectedResponse = Invoke-RestMethod -Uri "http://localhost:7218/api/reports/overview" -Method GET -Headers $headers
        Write-Host "Protected endpoint response:" -ForegroundColor Yellow
        $protectedResponse | ConvertTo-Json -Depth 3
    } else {
        Write-Host "No token received!" -ForegroundColor Red
    }
} catch {
    Write-Host "Error occurred:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    if ($_.Exception.Response) {
        Write-Host "Response status: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
    }
}
