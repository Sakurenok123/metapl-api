using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using MetaPlApi.Data.Entities;
using MetaPlApi.Services;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// ========== КОНФИГУРАЦИЯ ДЛЯ RAILWAY ==========
// Получаем URL фронтенда из окружения или используем Netlify по умолчанию
var frontendUrl = Environment.GetEnvironmentVariable("FRONTEND_URL") ?? "https://clever-basbousa-2c3b30.netlify.app";
var railwayPort = Environment.GetEnvironmentVariable("PORT") ?? "8080";

// Настраиваем Kestrel для Railway
builder.WebHost.UseKestrel(options =>
{
    options.ListenAnyIP(int.Parse(railwayPort));
});
Console.WriteLine($"✓ Railway environment detected, port: {railwayPort}");
Console.WriteLine($"✓ Frontend URL: {frontendUrl}");

// Переопределяем строку подключения из Railway
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
        Console.WriteLine($"✓ Database configured from DATABASE_URL");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"✗ Error parsing DATABASE_URL: {ex.Message}");
    }
}

// ========== ОСНОВНЫЕ СЕРВИСЫ ==========
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
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
if (!string.IsNullOrEmpty(connectionString))
{
    builder.Services.AddDbContext<MetaplatformeContext>(options =>
        options.UseNpgsql(connectionString));
    Console.WriteLine($"✓ Database context registered");
}
else
{
    Console.WriteLine($"✗ WARNING: Database connection string is not set!");
}

// ========== СЕРВИСЫ ==========
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPlaceService, PlaceService>();
builder.Services.AddScoped<IApplicationService, ApplicationService>();
builder.Services.AddScoped<IEventsService, EventsService>();
builder.Services.AddScoped<IStatusService, StatusService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddHttpContextAccessor();

// ========== CORS ==========
// Конкретная настройка CORS для Netlify и локальной разработки
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowNetlifyAndLocal", policy =>
    {
        policy.WithOrigins(
                "https://clever-basbousa-2c3b30.netlify.app", // Ваш Netlify домен
                "http://localhost:3000",                       // Локальный фронтенд
                "http://localhost:3001"
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()
            .WithExposedHeaders("Content-Disposition");
    });
});

// ========== СОЗДАНИЕ ПРИЛОЖЕНИЯ ==========
var app = builder.Build();

// Логирование конфигурации
Console.WriteLine($"=== MetaPl API Configuration ===");
Console.WriteLine($"Environment: {app.Environment.EnvironmentName}");
Console.WriteLine($"Port: {railwayPort}");
Console.WriteLine($"Frontend: {frontendUrl}");
Console.WriteLine($"Database configured: {!string.IsNullOrEmpty(connectionString)}");

// ========== КОНФИГУРАЦИЯ ПАЙПЛАЙНА ==========
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

// Включаем Swagger всегда для удобства тестирования
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "MetaPl API v1");
    c.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();

// ВАЖНО: CORS должен быть перед UseAuthorization и MapControllers
app.UseCors("AllowNetlifyAndLocal");

app.UseAuthentication();
app.UseAuthorization();

// ========== ОСНОВНЫЕ ЭНДПОИНТЫ ==========
app.MapGet("/", () => "MetaPl API is running. Use /swagger for API documentation.");
app.MapGet("/health", () => Results.Ok(new { 
    status = "Healthy", 
    timestamp = DateTime.UtcNow,
    environment = app.Environment.EnvironmentName,
    service = "MetaPl API",
    frontend = frontendUrl
}));

// Обработчик ошибок
app.MapGet("/error", () => Results.Problem("An error occurred"));

// ========== КОНТРОЛЛЕРЫ ==========
app.MapControllers();

// ========== ЗАПУСК ==========
Console.WriteLine($"=== Starting MetaPl API on port {railwayPort} ===");
app.Run();
