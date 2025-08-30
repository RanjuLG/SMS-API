using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SMS.Business;
using SMS.DBContext;
using SMS.Interfaces;
using SMS.Models;
using SMS.Repositories;
using SMS.Services;
using System.Text;
using static SMS.DBContext.ApplicationDbContext;

var builder = WebApplication.CreateBuilder(args);

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
            Console.WriteLine($"JWT OnMessageReceived - Authorization Header: {authHeader?.Substring(0, Math.Min(50, authHeader?.Length ?? 0))}...");
            if (!string.IsNullOrEmpty(context.Token))
            {
                Console.WriteLine($"JWT OnMessageReceived - Token extracted, length: {context.Token.Length}");
            }
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"JWT Authentication failed: {context.Exception.Message}");
            Console.WriteLine($"JWT Authentication failed - Exception Type: {context.Exception.GetType().Name}");
            if (context.Exception.InnerException != null)
            {
                Console.WriteLine($"JWT Authentication failed - Inner Exception: {context.Exception.InnerException.Message}");
            }
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            Console.WriteLine("JWT Token validated successfully");
            var claims = context.Principal?.Claims?.Select(c => $"{c.Type}: {c.Value}") ?? new string[0];
            Console.WriteLine($"JWT Claims: {string.Join(", ", claims)}");
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            Console.WriteLine($"JWT Challenge: {context.Error}, {context.ErrorDescription}");
            Console.WriteLine($"JWT Challenge - Auth Result: {context.AuthenticateFailure?.Message}");
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
builder.Services.AddScoped<BusinessLogic>();
builder.Services.AddTransient<IReadOnlyRepository, ReadOnlyRepository<ApplicationDbContext>>();

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

app.Run();

public class SeedData
{
    public static async Task Initialize(IServiceProvider serviceProvider, UserManager<ApplicationUser> userManager)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        // Define all roles in the system
        string[] roleNames = { "SuperAdmin", "Admin", "Cashier" };
        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
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
                Console.WriteLine("Super Admin user created successfully!");
                Console.WriteLine($"Username: {superAdmin.UserName}");
                Console.WriteLine($"Email: {superAdmin.Email}");
                Console.WriteLine($"Password: {superAdminPassword}");
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
                Console.WriteLine("Default Admin user created successfully!");
            }
        }
        else
        {
            // Upgrade existing admin to SuperAdmin if needed
            var currentRoles = await userManager.GetRolesAsync(adminUser);
            if (!currentRoles.Contains("SuperAdmin"))
            {
                await userManager.AddToRoleAsync(adminUser, "SuperAdmin");
                Console.WriteLine("Existing admin user upgraded to SuperAdmin!");
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
                Console.WriteLine("Default Cashier user created successfully!");
            }
        }
    }
}
