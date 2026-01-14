using BibliotecaApi.Data;
using Microsoft.EntityFrameworkCore;

// 🔐 JWT usings (NOVOS)
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

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

// Add DbContext for PostgreSQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// -----------------------------
// 🔐 JWT Authentication (NOVO)
// -----------------------------
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["Key"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // OK para desenvolvimento
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(secretKey!)
        ),

        ClockSkew = TimeSpan.Zero // sem tolerância para token expirado
    };
});

// Authorization service (necessário para [Authorize])
builder.Services.AddAuthorization();

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
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Redirect HTTP to HTTPS
app.UseHttpsRedirection();

// 🔐 Middleware JWT (ORDEM IMPORTA)
app.UseAuthentication();
app.UseAuthorization();

// -----------------------------
// Original code: WeatherForecast example
// -----------------------------
var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
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
app.MapControllers(); // <-- ESSENTIAL

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
