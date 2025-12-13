# Environment Configuration Quick Start

This guide provides quick instructions for setting up environment variables for the SMS API.

## ?? Prerequisites

- Database server (SQL Server)
- Generated JWT key (see below)
- Write access to environment variables

## ?? Quick Setup

### Windows (Recommended: Use the PowerShell Script)

1. **Generate a JWT Key** (if you don't have one):
   ```powershell
   $bytes = New-Object byte[] 32
   [System.Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($bytes)
   $key = [Convert]::ToBase64String($bytes)
   Write-Host "Your JWT Key: $key"
   ```

2. **Run the setup script**:
   ```powershell
   # For current user only
   .\Setup-Environment.ps1 -Scope User `
     -ConnectionString "Data Source=YOUR_SERVER;Initial Catalog=SMS;User Id=YOUR_USER;Password=YOUR_PASSWORD;MultipleActiveResultSets=True;TrustServerCertificate=True;" `
     -JwtKey "YOUR_GENERATED_JWT_KEY"

   # For system-wide (IIS, requires admin)
   .\Setup-Environment.ps1 -Scope System `
     -ConnectionString "Data Source=YOUR_SERVER;Initial Catalog=SMS;User Id=YOUR_USER;Password=YOUR_PASSWORD;MultipleActiveResultSets=True;TrustServerCertificate=True;" `
     -JwtKey "YOUR_GENERATED_JWT_KEY"
   ```

3. **Restart your terminal/IDE/IIS**

### Linux/macOS (Recommended: Use the Bash Script)

1. **Make the script executable**:
   ```bash
   chmod +x setup-environment.sh
   ```

2. **Generate a JWT Key** (if you don't have one):
   ```bash
   openssl rand -base64 32
   ```

3. **Run the setup script**:
   ```bash
   ./setup-environment.sh \
     --connection-string "Server=YOUR_SERVER;Database=SMS;User Id=YOUR_USER;Password=YOUR_PASSWORD;MultipleActiveResultSets=True;TrustServerCertificate=True;" \
     --jwt-key "YOUR_GENERATED_JWT_KEY" \
     --environment Production \
     --upload-path /var/www/sms/uploads/nic
   ```

4. **Reload your shell**:
   ```bash
   source ~/.bashrc  # or ~/.zshrc
   ```

## ?? Manual Setup

### Windows (Manual)

Open PowerShell and run:

```powershell
# User level (no admin required)
setx ASPNETCORE_ENVIRONMENT "Production"
setx SMS_ConnectionStrings__MasterDatabase "YOUR_CONNECTION_STRING"
setx SMS_Jwt__Key "YOUR_JWT_KEY"
setx SMS_Jwt__Issuer "SMS"
setx SMS_Jwt__Audience "SMS"
setx SMS_FileSettings__NicUploadFolderPath "C:\inetpub\wwwroot\SMS_GUI\brower\uploads\nic"

# System level (IIS, requires admin - add /M flag)
setx ASPNETCORE_ENVIRONMENT "Production" /M
setx SMS_ConnectionStrings__MasterDatabase "YOUR_CONNECTION_STRING" /M
# ... etc
```

### Linux/macOS (Manual)

Add to `~/.bashrc` or `~/.zshrc`:

```bash
export ASPNETCORE_ENVIRONMENT="Production"
export SMS_ConnectionStrings__MasterDatabase="YOUR_CONNECTION_STRING"
export SMS_Jwt__Key="YOUR_JWT_KEY"
export SMS_Jwt__Issuer="SMS"
export SMS_Jwt__Audience="SMS"
export SMS_FileSettings__NicUploadFolderPath="/var/www/sms/uploads/nic"
```

Then reload: `source ~/.bashrc`

## ?? Docker Setup

Add to `docker-compose.yml`:

```yaml
services:
  sms-api:
    image: sms-api:latest
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - SMS_ConnectionStrings__MasterDatabase=Server=db;Database=SMS;User Id=sa;Password=YourPassword123!
      - SMS_Jwt__Key=YOUR_JWT_KEY
      - SMS_Jwt__Issuer=SMS
      - SMS_Jwt__Audience=SMS
      - SMS_FileSettings__NicUploadFolderPath=/app/uploads/nic
    volumes:
      - ./uploads:/app/uploads
```

## ?? Azure App Service

1. Go to Azure Portal ? Your App Service ? **Configuration**
2. Click **"New application setting"**
3. Add each setting:
   - `SMS_ConnectionStrings__MasterDatabase`: Your connection string
   - `SMS_Jwt__Key`: Your JWT key
   - `SMS_Jwt__Issuer`: SMS
   - `SMS_Jwt__Audience`: SMS
   - `ASPNETCORE_ENVIRONMENT`: Production
4. Click **"Save"**
5. Restart the app

## ? Verification

1. **Check environment variables are set**:
   
   Windows:
   ```powershell
   [System.Environment]::GetEnvironmentVariable('SMS_Jwt__Issuer', 'User')
   ```
   
   Linux/macOS:
   ```bash
   echo $SMS_Jwt__Issuer
   ```

2. **Start the application and check logs**:
   Look for: `Database configuration: Configuration loaded`

3. **Test the health endpoint**:
   ```bash
   curl http://localhost:5000/api/health/database
   ```

## ?? Security Checklist

- [ ] Different credentials for dev/staging/production
- [ ] Strong passwords (12+ characters, mixed case, numbers, symbols)
- [ ] JWT key is at least 32 bytes (256 bits)
- [ ] Connection strings not committed to Git
- [ ] `.gitignore` configured properly
- [ ] System variables used for production (Windows)
- [ ] Proper file permissions on Linux (600 for config files)
- [ ] Regular credential rotation schedule

## ?? Connection String Examples

### SQL Server (Windows Auth)
```
Data Source=localhost;Initial Catalog=SMS;Integrated Security=True;MultipleActiveResultSets=True;TrustServerCertificate=True;
```

### SQL Server (SQL Auth)
```
Data Source=localhost;Initial Catalog=SMS;User Id=sa;Password=YourPassword123!;MultipleActiveResultSets=True;TrustServerCertificate=True;
```

### Azure SQL
```
Server=tcp:your-server.database.windows.net,1433;Initial Catalog=SMS;User Id=your-user@your-server;Password=YourPassword123!;Encrypt=True;TrustServerCertificate=False;MultipleActiveResultSets=True;
```

### SQL Server (Named Instance)
```
Data Source=localhost\SQLEXPRESS;Initial Catalog=SMS;Integrated Security=True;MultipleActiveResultSets=True;TrustServerCertificate=True;
```

## ?? Troubleshooting

### Variables not loading
- **Windows**: Restart terminal/IDE after setting variables
- **IIS**: Restart application pool or run `iisreset`
- **Linux**: Run `source ~/.bashrc` or restart terminal

### Permission errors
- **Windows**: Use `/M` flag for system variables (requires admin)
- **Linux**: Check file permissions: `chmod 755` for directories, `chmod 644` for files

### Database connection fails
- Verify SQL Server is running
- Check firewall rules
- Test connection string manually
- Verify user has database access
- Check if TCP/IP is enabled in SQL Server Configuration

### JWT errors
- Ensure key is base64 encoded
- Key must be at least 32 bytes when decoded
- No spaces or line breaks in the key

## ?? Additional Resources

- [Full Documentation](./ENVIRONMENT_SETUP.md) - Detailed configuration guide
- [Template File](./.env.template) - Environment variables template
- [.gitignore](./.gitignore) - Protected files configuration

## ?? Environment Migration

### From appsettings.json to Environment Variables

1. **Backup current settings**:
   ```bash
   cp appsettings.json appsettings.json.backup
   ```

2. **Extract values** from `appsettings.json`

3. **Set as environment variables** using scripts above

4. **Clear sensitive values** from `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "MasterDatabase": ""
     },
     "Jwt": {
       "Key": ""
     }
   }
   ```

5. **Test** the application

6. **Commit** the cleaned `appsettings.json`

## ?? Support

For issues or questions:
- Check logs in `logs/` directory
- Review [ENVIRONMENT_SETUP.md](./ENVIRONMENT_SETUP.md) for detailed info
- Open an issue on the repository

---

**Security Reminder**: Never commit actual credentials to source control!
