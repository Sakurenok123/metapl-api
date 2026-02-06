using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using MetaPlApi.Data.Entities;
using MetaPlApi.Services;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Конфигурация для Railway
var railwayPort = Environment.GetEnvironmentVariable("PORT") ?? "8080";
Console.WriteLine($"Railway port: {railwayPort}");

// Переопределяем строку подключения из Railway
var railwayDbUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
if (!string.IsNullOrEmpty(railwayDbUrl))
{
    try
    {
        var uri = new Uri(railwayDbUrl);
        var userInfo = uri.UserInfo.Split(':');
        
        var connectionString = $"Host={uri.Host};" +
                             $"Database={uri.AbsolutePath.TrimStart('/')};" +
                             $"Username={userInfo[0]};" +
                             $"Password={userInfo[1]};" +
                             $"Port={uri.Port};" +
                             "SSL Mode=Require;Trust Server Certificate=true;";
        
        builder.Configuration["ConnectionStrings:DefaultConnection"] = connectionString;
        Console.WriteLine($"✓ Database configured from DATABASE_URL");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"✗ Error parsing DATABASE_URL: {ex.Message}");
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
        Version = "v1" 
    });
});

// База данных
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (!string.IsNullOrEmpty(connectionString))
{
    builder.Services.AddDbContext<MetaplatformeContext>(options =>
        options.UseNpgsql(connectionString));
    Console.WriteLine($"✓ Database context registered");
}
else
{
    Console.WriteLine($"✗ WARNING: No database connection string!");
}

// Сервисы
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPlaceService, PlaceService>();
builder.Services.AddScoped<IApplicationService, ApplicationService>();
builder.Services.AddScoped<IEventsService, EventsService>();
builder.Services.AddScoped<IStatusService, StatusService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddHttpContextAccessor();

// CORS - конкретная настройка для вашего Netlify
builder.Services.AddCors(options =>
{
    options.AddPolicy("NetlifyCors", policy =>
    {
        policy.WithOrigins(
                "https://clever-basbousa-2c3b30.netlify.app",
                "http://localhost:3000",
                "http://localhost:3001"
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
    
    // Также добавляем AllowAll для тестирования
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Создание приложения
var app = builder.Build();

// Middleware в правильном порядке
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// ВАЖНО: CORS должен быть ПЕРЕД UseAuthorization и MapControllers
app.UseCors("AllowAll"); // Используем AllowAll для тестирования

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "MetaPl API v1");
    c.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Тестовые endpoint'ы
app.MapGet("/", () => "MetaPl API is running!");
app.MapGet("/test", () => new { 
    status = "OK", 
    time = DateTime.UtcNow,
    cors = "AllowAll enabled",
    database = !string.IsNullOrEmpty(connectionString) ? "Connected" : "Not connected"
});
app.MapGet("/health", () => Results.Ok(new { 
    status = "Healthy", 
    timestamp = DateTime.UtcNow,
    environment = app.Environment.EnvironmentName,
    api = "MetaPl API"
}));

Console.WriteLine("=== MetaPl API Starting ===");
app.Run();
