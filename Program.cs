using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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
builder.Services.AddSwaggerGen();

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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

// Configure Role-based Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminPolicy", policy => policy.RequireRole("Admin"));
    options.AddPolicy("CashierPolicy", policy => policy.RequireRole("Cashier"));
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

        string[] roleNames = { "Admin", "Cashier" };
        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        var admin = new ApplicationUser
        {
            UserName = "admin",
            Email = "admin@example.com"
        };

        var adminPassword = "Admin@123";
        var adminUser = await userManager.FindByNameAsync(admin.UserName);
        if (adminUser == null)
        {
            var createAdmin = await userManager.CreateAsync(admin, adminPassword);
            if (createAdmin.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, "Admin");
            }
        }
    }
}
