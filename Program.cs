using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SMS.Business;
using SMS.DBContext;
using SMS.Interfaces;
using SMS.Models;
using SMS.Models.Configuration;
using SMS.Repositories;
using SMS.Services;
using System.Text;
using static SMS.DBContext.ApplicationDbContext;
using Serilog;

// Configure Serilog early
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json")
        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
        .Build())
    .CreateLogger();

try
{
    Log.Information("Starting SMS API application");

    var builder = WebApplication.CreateBuilder(args);

    // Configure Serilog
    builder.Host.UseSerilog();

    // Add services to the container.
    builder.Services.AddControllers();
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("CorsPolicy",
           builder => builder
           .AllowAnyOrigin()
           .AllowAnyMethod()
           .AllowAnyHeader());
    });


// Configure Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo 
    { 
        Title = "SMS API", 
        Version = "v1",
        Description = "Store Management System API for gold/jewelry pawn shop operations"
    });
    
    // Add JWT Bearer authentication to Swagger
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// Configure DbContext with lazy loading
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MasterDatabase"))
           .UseLazyLoadingProxies());

// Configure Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Configure JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(builder.Configuration["Jwt:Key"] ?? "R2VuZXJpY0RlZmF1bHRTZWNyZXRLZXk=")),
        ClockSkew = TimeSpan.FromMinutes(5), // Allow 5 minutes clock skew
        RequireExpirationTime = true,
        ValidateActor = false,
        ValidateTokenReplay = false
    };

    // Add comprehensive event handlers for debugging
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
            Log.Debug("JWT OnMessageReceived - Authorization Header: {AuthHeader}", authHeader?.Substring(0, Math.Min(50, authHeader?.Length ?? 0)) + "...");
            if (!string.IsNullOrEmpty(context.Token))
            {
                Log.Debug("JWT OnMessageReceived - Token extracted, length: {TokenLength}", context.Token.Length);
            }
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            Log.Error("JWT Authentication failed: {ErrorMessage}", context.Exception.Message);
            Log.Error("JWT Authentication failed - Exception Type: {ExceptionType}", context.Exception.GetType().Name);
            if (context.Exception.InnerException != null)
            {
                Log.Error("JWT Authentication failed - Inner Exception: {InnerException}", context.Exception.InnerException.Message);
            }
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            Log.Information("JWT Token validated successfully");
            var claims = context.Principal?.Claims?.Select(c => $"{c.Type}: {c.Value}") ?? new string[0];
            Log.Debug("JWT Claims: {Claims}", string.Join(", ", claims));
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            Log.Warning("JWT Challenge: {Error}, {ErrorDescription}", context.Error, context.ErrorDescription);
            Log.Warning("JWT Challenge - Auth Result: {AuthFailure}", context.AuthenticateFailure?.Message);
            return Task.CompletedTask;
        }
    };
});

// Configure Role-based Authorization
builder.Services.AddAuthorization(options =>
{
    // SuperAdmin has access to everything
    options.AddPolicy("SuperAdminPolicy", policy => policy.RequireRole("SuperAdmin"));
    
    // Admin policy allows both SuperAdmin and Admin roles
    options.AddPolicy("AdminPolicy", policy => policy.RequireRole("SuperAdmin", "Admin"));
    
    // Cashier policy allows SuperAdmin, Admin, and Cashier roles
    options.AddPolicy("CashierPolicy", policy => policy.RequireRole("SuperAdmin", "Admin", "Cashier"));
    
    // User management (only SuperAdmin and Admin)
    options.AddPolicy("UserManagementPolicy", policy => policy.RequireRole("SuperAdmin", "Admin"));
    
    // System management (only SuperAdmin)
    options.AddPolicy("SystemManagementPolicy", policy => policy.RequireRole("SuperAdmin"));
});

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(Program).Assembly);

// Add Memory Cache for health monitoring
builder.Services.AddMemoryCache();

// Configure health monitoring settings
builder.Services.Configure<HealthMonitoringConfiguration>(
    builder.Configuration.GetSection("HealthMonitoring"));

// Register application services and repositories
builder.Services.AddTransient<IRepository, Repository<ApplicationDbContext>>();
builder.Services.AddTransient<ICustomerService, CustomerService>();
builder.Services.AddTransient<IItemService, ItemService>();
builder.Services.AddTransient<IInvoiceService, InvoiceService>();
builder.Services.AddTransient<ITransactionService, TransactionService>();
builder.Services.AddTransient<ITransactionItemService, TransactionItemService>();
builder.Services.AddTransient<IKaratageService, KaratageService>();
builder.Services.AddTransient<IInstallmentService, InstallmentService>();
builder.Services.AddTransient<ILoanService, LoanService>();
builder.Services.AddTransient<IPaginationService, PaginationService>();
builder.Services.AddTransient<IHealthService, HealthService>();
builder.Services.AddScoped<BusinessLogic>();
builder.Services.AddTransient<IReadOnlyRepository, ReadOnlyRepository<ApplicationDbContext>>();

// Register Background Services for Health Monitoring (Conditionally based on configuration)
var healthConfig = builder.Configuration.GetSection("HealthMonitoring").Get<HealthMonitoringConfiguration>() ?? new HealthMonitoringConfiguration();

if (healthConfig.BackgroundServices.EnableDatabaseBackup)
{
    builder.Services.AddHostedService<SMS.Services.Background.DatabaseBackupService>();
}

if (healthConfig.BackgroundServices.EnableHealthDataCollection)
{
    builder.Services.AddHostedService<SMS.Services.Background.HealthDataCollectionService>();
}

if (healthConfig.BackgroundServices.EnableHealthAlerting)
{
    builder.Services.AddHostedService<SMS.Services.Background.HealthAlertingService>();
}

if (healthConfig.BackgroundServices.EnableLogCleanup)
{
    builder.Services.AddHostedService<SMS.Services.Background.LogCleanupService>();
}

if (healthConfig.BackgroundServices.EnableCertificateMonitoring)
{
    builder.Services.AddHostedService<SMS.Services.Background.CertificateMonitoringService>();
}

    var app = builder.Build();

    // Seed roles and admin user
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        await SeedData.Initialize(services, userManager);
    }

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseDeveloperExceptionPage();
    }

    app.UseCors("CorsPolicy");
    app.UseHttpsRedirection();
    app.UseStaticFiles(); // Make sure this is enabled
    app.UseRouting();
    app.UseAuthentication();  // Enable authentication middleware
    app.UseAuthorization();   // Enable authorization middleware

    app.MapControllers();

    Log.Information("SMS API application started successfully");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "SMS API application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

public class SeedData
{
    public static async Task Initialize(IServiceProvider serviceProvider, UserManager<ApplicationUser> userManager)
    {
        var logger = serviceProvider.GetRequiredService<ILogger<SeedData>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        // Define all roles in the system
        string[] roleNames = { "SuperAdmin", "Admin", "Cashier" };
        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
                logger.LogInformation("Created role: {RoleName}", roleName);
            }
        }

        // Create Super Admin user
        var superAdmin = new ApplicationUser
        {
            UserName = "superadmin",
            Email = "superadmin@sms.com",
            EmailConfirmed = true,
            LockoutEnabled = false
        };

        var superAdminPassword = "SuperAdmin@123!";
        var existingSuperAdmin = await userManager.FindByNameAsync(superAdmin.UserName);
        if (existingSuperAdmin == null)
        {
            var createSuperAdmin = await userManager.CreateAsync(superAdmin, superAdminPassword);
            if (createSuperAdmin.Succeeded)
            {
                await userManager.AddToRoleAsync(superAdmin, "SuperAdmin");
                logger.LogInformation("Super Admin user created successfully with username: {Username} and email: {Email}", 
                    superAdmin.UserName, superAdmin.Email);
            }
            else
            {
                logger.LogError("Failed to create Super Admin user: {Errors}", 
                    string.Join(", ", createSuperAdmin.Errors.Select(e => e.Description)));
            }
        }

        // Create default Admin user (legacy compatibility)
        var admin = new ApplicationUser
        {
            UserName = "admin",
            Email = "admin@example.com",
            EmailConfirmed = true
        };

        var adminPassword = "Admin@123";
        var adminUser = await userManager.FindByNameAsync(admin.UserName);
        if (adminUser == null)
        {
            var createAdmin = await userManager.CreateAsync(admin, adminPassword);
            if (createAdmin.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, "Admin");
                logger.LogInformation("Default Admin user created successfully");
            }
            else
            {
                logger.LogError("Failed to create Admin user: {Errors}", 
                    string.Join(", ", createAdmin.Errors.Select(e => e.Description)));
            }
        }
        else
        {
            // Upgrade existing admin to SuperAdmin if needed
            var currentRoles = await userManager.GetRolesAsync(adminUser);
            if (!currentRoles.Contains("SuperAdmin"))
            {
                await userManager.AddToRoleAsync(adminUser, "SuperAdmin");
                logger.LogInformation("Existing admin user upgraded to SuperAdmin");
            }
        }

        // Create default Cashier user for testing
        var cashier = new ApplicationUser
        {
            UserName = "cashier",
            Email = "cashier@example.com",
            EmailConfirmed = true
        };

        var cashierPassword = "Cashier@123";
        var cashierUser = await userManager.FindByNameAsync(cashier.UserName);
        if (cashierUser == null)
        {
            var createCashier = await userManager.CreateAsync(cashier, cashierPassword);
            if (createCashier.Succeeded)
            {
                await userManager.AddToRoleAsync(cashier, "Cashier");
                logger.LogInformation("Default Cashier user created successfully");
            }
            else
            {
                logger.LogError("Failed to create Cashier user: {Errors}", 
                    string.Join(", ", createCashier.Errors.Select(e => e.Description)));
            }
        }
    }
}
