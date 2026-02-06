using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using MetaPlApi.Data.Entities;
using MetaPlApi.Services;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Конфигурация для Railway
var railwayPort = Environment.GetEnvironmentVariable("PORT") ?? "8080";
Console.WriteLine($"🚀 Railway port: {railwayPort}");

// 1. Сначала проверим переменные окружения
Console.WriteLine("=== Environment Variables ===");
Console.WriteLine($"DATABASE_URL exists: {!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DATABASE_URL"))}");
Console.WriteLine($"RAILWAY_ENVIRONMENT: {Environment.GetEnvironmentVariable("RAILWAY_ENVIRONMENT")}");

// 2. Получаем строку подключения из DATABASE_URL
string connectionString = "";

var railwayDbUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
if (!string.IsNullOrEmpty(railwayDbUrl))
{
    Console.WriteLine($"🔗 Found DATABASE_URL: {railwayDbUrl.Substring(0, Math.Min(railwayDbUrl.Length, 50))}...");
    
    try
    {
        var uri = new Uri(railwayDbUrl);
        var userInfo = uri.UserInfo.Split(':');
        
        connectionString = $"Host={uri.Host};" +
                         $"Database={uri.AbsolutePath.TrimStart('/')};" +
                         $"Username={userInfo[0]};" +
                         $"Password={userInfo[1]};" +
                         $"Port={uri.Port};" +
                         "SSL Mode=Require;Trust Server Certificate=true;";
        
        Console.WriteLine($"✅ Database connection string configured");
        Console.WriteLine($"📊 Host: {uri.Host}");
        Console.WriteLine($"📊 Database: {uri.AbsolutePath.TrimStart('/')}");
        Console.WriteLine($"📊 Username: {userInfo[0]}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error parsing DATABASE_URL: {ex.Message}");
    }
}
else
{
    Console.WriteLine("❌ DATABASE_URL not found in environment variables");
    Console.WriteLine("📋 Available environment variables:");
    foreach (var key in Environment.GetEnvironmentVariables().Keys)
    {
        Console.WriteLine($"  {key}");
    }
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
        Version = "v1",
        Description = "API для платформы метаплатформ"
    });
});

// База данных
if (!string.IsNullOrEmpty(connectionString))
{
    Console.WriteLine($"🔌 Registering database context with connection string");
    builder.Services.AddDbContext<MetaplatformeContext>(options =>
    {
        options.UseNpgsql(connectionString);
        options.EnableSensitiveDataLogging(true); // Для отладки
    });
}
else
{
    Console.WriteLine($"⚠️  No database connection string. Using in-memory database for testing.");
    // Не добавляем базу данных вообще, будем работать без нее
}

// Сервисы
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPlaceService, PlaceService>();
builder.Services.AddScoped<IApplicationService, ApplicationService>();
builder.Services.AddScoped<IEventsService, EventsService>();
builder.Services.AddScoped<IStatusService, StatusService>();
builder.Services.AddScoped<IAuthService, AuthService>();
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

// Отключаем HTTPS редирект для Railway (они сами обрабатывают SSL)
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

// Проверка переменных окружения
app.MapGet("/env", () =>
{
    var envVars = new Dictionary<string, string?>();
    foreach (System.Collections.DictionaryEntry de in Environment.GetEnvironmentVariables())
    {
        if (de.Key.ToString()?.Contains("DATABASE") == true || 
            de.Key.ToString()?.Contains("RAILWAY") == true ||
            de.Key.ToString()?.Contains("URL") == true ||
            de.Key.ToString()?.Contains("PORT") == true)
        {
            envVars[de.Key.ToString()!] = de.Value?.ToString();
        }
    }
    return Results.Ok(envVars);
});

Console.WriteLine($"=== MetaPl API Starting on port {railwayPort} ===");
Console.WriteLine($"=== Environment: {app.Environment.EnvironmentName} ===");
Console.WriteLine($"=== Database: {(string.IsNullOrEmpty(connectionString) ? "Not configured" : "Configured")} ===");

app.Run();
