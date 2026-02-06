using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using MetaPlApi.Data.Entities;
using MetaPlApi.Services;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// ВАЖНО ДЛЯ RAILWAY: Устанавливаем порт до создания app
var railwayPort = Environment.GetEnvironmentVariable("PORT") ?? "8080";
Console.WriteLine($"🚀 Railway port: {railwayPort}");

// Устанавливаем URL для Kestrel
builder.WebHost.UseUrls($"http://*:{railwayPort}");
Console.WriteLine($"🔗 Kestrel will listen on: http://*:{railwayPort}");

// Проверяем переменные окружения
Console.WriteLine("=== Environment Variables ===");
Console.WriteLine($"PORT: {railwayPort}");
Console.WriteLine($"ASPNETCORE_ENVIRONMENT: {Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}");

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
        // Для Railway лучше не использовать localhost
        connectionString = "";
    }
}
else
{
    Console.WriteLine($"⚠️  DATABASE_URL not found, running without database");
    connectionString = "";
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
        Description = "API для платформы метаплатформ",
        Contact = new OpenApiContact
        {
            Name = "MetaPl Team",
            Email = "support@metapl.ru"
        }
    });
});

// База данных - только если есть строка подключения
if (!string.IsNullOrEmpty(connectionString))
{
    Console.WriteLine($"🔌 Registering database with connection string");
    builder.Services.AddDbContext<MetaplatformeContext>(options =>
    {
        options.UseNpgsql(connectionString);
        options.LogTo(Console.WriteLine, Microsoft.Extensions.Logging.LogLevel.Information);
    });
}
else
{
    Console.WriteLine($"⚠️  No database connection, using in-memory for testing");
    // Используем in-memory для тестов
    builder.Services.AddDbContext<MetaplatformeContext>(options =>
    {
        options.UseInMemoryDatabase("MetaPlTestDB");
    });
}

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

// ВАЖНО: Указываем, что мы в Production для Railway
app.Environment.EnvironmentName = "Production";

// Middleware
app.UseDeveloperExceptionPage(); // Всегда включаем для отладки в Railway

// ВАЖНО: CORS должен быть ПЕРЕД UseAuthorization и MapControllers
app.UseCors("AllowAll");

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "MetaPl API v1");
    c.RoutePrefix = "swagger";
    c.DisplayRequestDuration();
});

// Отключаем HTTPS редирект для Railway
// app.UseHttpsRedirection();

app.UseAuthorization();

// ВАЖНО: MapControllers должен быть ДО MapGet для /
app.MapControllers();

// Тестовые endpoint'ы
app.MapGet("/", () => 
{
    var baseUrl = $"{app.Environment.EnvironmentName} - Port: {railwayPort}";
    return $"✅ MetaPl API is running! {baseUrl}";
});

app.MapGet("/test", () => new 
{ 
    status = "OK", 
    time = DateTime.UtcNow,
    environment = app.Environment.EnvironmentName,
    port = railwayPort,
    database = !string.IsNullOrEmpty(connectionString) ? "Configured" : "Test mode (in-memory)",
    api = "MetaPl API",
    version = "1.0",
    urls = new[] { 
        "/swagger", 
        "/health", 
        "/api/places", 
        "/api/applications" 
    }
});

app.MapGet("/health", () => Results.Ok(new 
{ 
    status = "Healthy", 
    timestamp = DateTime.UtcNow,
    environment = app.Environment.EnvironmentName,
    api = "MetaPl API",
    version = "1.0"
}));

// Endpoint для проверки переменных окружения (только ключевые)
app.MapGet("/env", () =>
{
    var envVars = Environment.GetEnvironmentVariables();
    var filtered = new Dictionary<string, string?>();
    
    foreach (System.Collections.DictionaryEntry entry in envVars)
    {
        var key = entry.Key.ToString();
        if (key?.Contains("PORT", StringComparison.OrdinalIgnoreCase) == true ||
            key?.Contains("RAILWAY", StringComparison.OrdinalIgnoreCase) == true ||
            key?.Contains("DATABASE", StringComparison.OrdinalIgnoreCase) == true ||
            key?.Contains("URL", StringComparison.OrdinalIgnoreCase) == true)
        {
            filtered[key] = entry.Value?.ToString();
        }
    }
    
    return Results.Ok(filtered);
});

// Endpoint для проверки базовых контроллеров
app.MapGet("/api-check", () =>
{
    var controllers = new[]
    {
        "/api/places",
        "/api/applications",
        "/api/auth",
        "/api/users"
    };
    
    return Results.Ok(new
    {
        message = "API endpoints available",
        endpoints = controllers,
        timestamp = DateTime.UtcNow
    });
});

Console.WriteLine($"=== MetaPl API Starting on port {railwayPort} ===");
Console.WriteLine($"=== Environment: {app.Environment.EnvironmentName} ===");
Console.WriteLine($"=== Swagger UI: http://localhost:{railwayPort}/swagger ===");
Console.WriteLine($"=== Health check: http://localhost:{railwayPort}/health ===");
Console.WriteLine($"=== Root endpoint: http://localhost:{railwayPort}/ ===");

app.Run();
