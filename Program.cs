using PharmacyApi.Services;
using PharmacyApi.Validators;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddScoped<IPrescriptionService, PrescriptionService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<PrescriptionValidator>();

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
