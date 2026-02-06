using MetaPlApi.Controllers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MetaPlApi.Data.Entities;
using MetaPlApi.Services;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// ========== КОНФИГУРАЦИЯ ДЛЯ RAILWAY ==========
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(int.Parse(port));
});

// Переопределяем строку подключения
var railwayDbUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
if (!string.IsNullOrEmpty(railwayDbUrl))
{
    try
    {
        var uri = new Uri(railwayDbUrl);
        var userInfo = uri.UserInfo.Split(':');
        
        var updatedConnectionString = $"Host={uri.Host};" +
                                     $"Database={uri.AbsolutePath.TrimStart('/')};" +
                                     $"Username={userInfo[0]};" +
                                     $"Password={userInfo[1]};" +
                                     $"Port={uri.Port};" +
                                     "SSL Mode=Require;Trust Server Certificate=true;";
        
        builder.Configuration["ConnectionStrings:DefaultConnection"] = updatedConnectionString;
        Console.WriteLine($"Database configured from DATABASE_URL");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error parsing DATABASE_URL: {ex.Message}");
    }
}

// ========== ОСНОВНЫЕ СЕРВИСЫ ==========
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.WriteIndented = builder.Environment.IsDevelopment();
    });

builder.Services.AddEndpointsApiExplorer();

// Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "MetaPl API", 
        Version = "v1",
        Description = "API для платформы метаплатформ"
    });
});

// ========== БАЗА ДАННЫХ ==========
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
Console.WriteLine($"Connection string: {(string.IsNullOrEmpty(connectionString) ? "NOT SET" : "SET")}");

if (!string.IsNullOrEmpty(connectionString))
{
    builder.Services.AddDbContext<MetaplatformeContext>(options =>
        options.UseNpgsql(connectionString));
}
else
{
    Console.WriteLine("WARNING: Database connection string is not set!");
}

// ========== JWT АУТЕНТИФИКАЦИЯ ==========
var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") ?? 
                builder.Configuration["Jwt:Secret"];
var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? 
                builder.Configuration["Jwt:Issuer"] ?? "https://metaplatforme.ru/";
var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? 
                  builder.Configuration["Jwt:Audience"] ?? "https://metaplatforme.ru/";

if (!string.IsNullOrEmpty(jwtSecret))
{
    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = !string.IsNullOrEmpty(jwtIssuer),
            ValidateAudience = !string.IsNullOrEmpty(jwtAudience),
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };
    });
    Console.WriteLine("JWT authentication configured");
}
else
{
    Console.WriteLine("WARNING: JWT Secret is not set!");
}

builder.Services.AddAuthorization();

// ========== СЕРВИСЫ ==========
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPlaceService, PlaceService>();
builder.Services.AddScoped<IApplicationService, ApplicationService>();
builder.Services.AddScoped<IEventsService, EventsService>();
builder.Services.AddScoped<IStatusService, StatusService>();
builder.Services.AddHttpContextAccessor();

// ========== CORS ==========
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// ========== ПРИЛОЖЕНИЕ ==========
var app = builder.Build();

// Логирование конфигурации
Console.WriteLine($"Environment: {app.Environment.EnvironmentName}");
Console.WriteLine($"Port: {port}");
Console.WriteLine($"Database URL: {(string.IsNullOrEmpty(railwayDbUrl) ? "NOT SET" : "SET")}");
Console.WriteLine($"JWT Secret: {(string.IsNullOrEmpty(jwtSecret) ? "NOT SET" : "SET")}");

// Обработка ошибок
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "MetaPl API v1");
        c.RoutePrefix = "swagger";
    });
}
else
{
    app.UseExceptionHandler("/error");
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

// ========== ЭНДПОИНТЫ ==========
app.MapGet("/", () => "MetaPl API is running");
app.MapGet("/health", () => Results.Ok(new { 
    status = "Healthy", 
    timestamp = DateTime.UtcNow,
    environment = app.Environment.EnvironmentName 
}));
app.MapGet("/error", () => Results.Problem("An error occurred"));

app.MapControllers();

// Запуск
Console.WriteLine($"Starting application on port {port}...");
app.Run();
