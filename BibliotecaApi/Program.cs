using BibliotecaApi.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// -----------------------------
// Register services for the application
// -----------------------------

// Add controllers (required to use [ApiController] and MapControllers)
builder.Services.AddControllers();  // <-- ESSENTIAL

// -----------------------------
// Add Swagger / OpenAPI services
// -----------------------------
builder.Services.AddEndpointsApiExplorer();  // <-- Required for Swagger to discover endpoints
builder.Services.AddSwaggerGen();            // <-- Generates the Swagger/OpenAPI documentation

// Add DbContext for PostgreSQL (optional for now, but required later)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// -----------------------------
// Build the application
// -----------------------------
var app = builder.Build();

// -----------------------------
// Configure the HTTP request pipeline
// -----------------------------
if (app.Environment.IsDevelopment())
{
    // Enable Swagger UI and JSON in development
    app.UseSwagger();       // <-- Serves the Swagger JSON endpoint
    app.UseSwaggerUI();     // <-- Serves the interactive Swagger UI at /swagger
}

// Redirect HTTP to HTTPS
app.UseHttpsRedirection();

// Original code: WeatherForecast example
var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

// -----------------------------
// Map controller routes
// -----------------------------
app.MapControllers(); // <-- ESSENTIAL, allows /authors and other controllers to work

// -----------------------------
// Run the application
// -----------------------------
app.Run();

// -----------------------------
// Record type used in WeatherForecast example
// -----------------------------
record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
