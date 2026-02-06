using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using MetaPlApi.Data.Entities;
using MetaPlApi.Services;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Конфигурация для Railway
var railwayPort = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://*:{railwayPort}");

Console.WriteLine($"Starting on port: {railwayPort}");

// База данных
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
        Console.WriteLine($"✓ Database configured");
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
    builder.Services.AddDbContext<MetaplatformeContext>(options =>
        options.UseInMemoryDatabase("TestDb"));
    Console.WriteLine($"✓ Using in-memory database for testing");
}

// Сервисы
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPlaceService, PlaceService>();
builder.Services.AddScoped<IApplicationService, ApplicationService>();
builder.Services.AddScoped<IEventsService, EventsService>();
builder.Services.AddScoped<IStatusService, StatusService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddHttpContextAccessor();

// CORS - разрешаем всё для тестирования
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

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "MetaPl API v1");
    c.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();

// CORS должен быть здесь
app.UseCors("AllowAll");

app.UseAuthorization();
app.MapControllers();

// Тестовый endpoint
app.MapGet("/", () => "MetaPl API is running!");
app.MapGet("/test", () => new { status = "OK", time = DateTime.UtcNow });

Console.WriteLine("=== MetaPl API Starting ===");
app.Run();
