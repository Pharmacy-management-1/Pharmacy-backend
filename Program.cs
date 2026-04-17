using PharmacyApi.Services;
var builder = WebApplication.CreateBuilder(args);


builder.Services.AddScoped<IOrderHistoryService, OrderHistoryService>();
builder.Services.AddScoped<IQuickReorderService, QuickReorderService>();
builder.Services.AddScoped<IHealthPackageService, HealthPackageService>();
builder.Services.AddScoped<IOfferService, OfferService>();
builder.Services.AddScoped<IEmailService, EmailService>();

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddScoped<IPrescriptionService, PrescriptionService>();
builder.Services.AddScoped<IOrderService, OrderService>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();
var uploadsFolder = Path.Combine(app.Environment.WebRootPath ?? "wwwroot", "prescriptions");
if (!Directory.Exists(uploadsFolder))
    Directory.CreateDirectory(uploadsFolder);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
