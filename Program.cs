using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using MetaPlApi.Data.Entities;
using MetaPlApi.Services;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Конфигурация для Railway
var railwayPort = Environment.GetEnvironmentVariable("PORT") ?? "8080";
Console.WriteLine($"🚀 Railway port: {railwayPort}");

// Проверяем переменные окружения
Console.WriteLine("=== Environment Variables ===");
var railwayDbUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
Console.WriteLine($"DATABASE_URL exists: {!string.IsNullOrEmpty(railwayDbUrl)}");

string connectionString = "";

if (!string.IsNullOrEmpty(railwayDbUrl))
{
    try
    {
        // Преобразуем DATABASE_URL в строку подключения для Npgsql
        var uri = new Uri(railwayDbUrl);
        var userInfo = uri.UserInfo.Split(':');
        
        connectionString = $"Host={uri.Host};" +
                         $"Database={uri.AbsolutePath.TrimStart('/')};" +
                         $"Username={userInfo[0]};" +
                         $"Password={userInfo[1]};" +
                         $"Port={uri.Port};" +
                         "SSL Mode=Require;Trust Server Certificate=true;";
        
        Console.WriteLine($"✅ Database connection configured");
        Console.WriteLine($"📊 Host: {uri.Host}");
        Console.WriteLine($"📊 Database: {uri.AbsolutePath.TrimStart('/')}");
        Console.WriteLine($"📊 Username: {userInfo[0]}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error parsing DATABASE_URL: {ex.Message}");
        connectionString = "Host=localhost;Database=test;Username=postgres;Password=1234";
    }
}
else
{
    Console.WriteLine($"⚠️  DATABASE_URL not found, using default connection");
    connectionString = "Host=localhost;Database=test;Username=postgres;Password=1234";
}

// Основные сервисы
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.WriteIndented = true;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "MetaPl API", 
        Version = "v1" 
    });
});

// База данных
Console.WriteLine($"🔌 Registering database with connection string");
builder.Services.AddDbContext<MetaplatformeContext>(options =>
{
    options.UseNpgsql(connectionString);
    options.LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information);
});

// Сервисы
builder.Services.AddScoped<IPlaceService, PlaceService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IApplicationService, ApplicationService>();
builder.Services.AddScoped<IEventsService, EventsService>();
builder.Services.AddScoped<IStatusService, StatusService>();
builder.Services.AddHttpContextAccessor();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Создание приложения
var app = builder.Build();

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// ВАЖНО: CORS должен быть ПЕРЕД UseAuthorization и MapControllers
app.UseCors("AllowAll");

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "MetaPl API v1");
    c.RoutePrefix = "swagger";
});

// Отключаем HTTPS редирект для Railway
// app.UseHttpsRedirection();

app.UseAuthorization();
app.MapControllers();

// Тестовые endpoint'ы
app.MapGet("/", () => "✅ MetaPl API is running!");
app.MapGet("/test", () => new 
{ 
    status = "OK", 
    time = DateTime.UtcNow,
    environment = app.Environment.EnvironmentName,
    port = railwayPort,
    database = !string.IsNullOrEmpty(connectionString) ? "Configured" : "Not configured"
});

app.MapGet("/health", () => Results.Ok(new 
{ 
    status = "Healthy", 
    timestamp = DateTime.UtcNow,
    environment = app.Environment.EnvironmentName,
    api = "MetaPl API",
    version = "1.0"
}));

Console.WriteLine($"=== MetaPl API Starting on port {railwayPort} ===");
app.Run();
