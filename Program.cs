using Microsoft.EntityFrameworkCore;
using SMS.DBContext;
using SMS.Interfaces;
using SMS.Repositories;
using SMS.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
                      builder =>
                      {
                          builder.WithOrigins("http://localhost:4200")
                                 .AllowAnyHeader()
                                 .AllowAnyMethod();
                      });
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MasterDatabase"))
           .UseLazyLoadingProxies()); // Enable lazy loading proxies

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(Program).Assembly);
builder.Services.AddTransient<IRepository, Repository<ApplicationDbContext>>();
builder.Services.AddTransient<ICustomerService, CustomerService>();
builder.Services.AddTransient<IItemService, ItemService>();
builder.Services.AddTransient<IInvoiceService, InvoiceService>();
builder.Services.AddTransient<ITransactionService, TransactionService>();
builder.Services.AddTransient<ITransactionItemService, TransactionItemService>();
builder.Services.AddTransient<ICaratageService, CaratageService>();
builder.Services.AddTransient<IReadOnlyRepository, ReadOnlyRepository<ApplicationDbContext>>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseRouting();
app.UseCors("AllowSpecificOrigin");
app.UseAuthorization();
app.MapControllers();
app.Run();
