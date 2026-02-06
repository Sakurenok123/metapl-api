using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MetaPlApi.Data.Entities;
using MetaPlApi.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ========== КОНФИГУРАЦИЯ ДЛЯ RAILWAY ==========
// Получаем порт из переменной окружения (Railway устанавливает PORT)
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";

// Конфигурируем Kestrel для работы на Railway
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(int.Parse(port));
});

// ========== НАСТРОЙКА КОНФИГУРАЦИИ ==========
// Переопределяем строку подключения из переменных окружения Railway
var railwayDbUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
if (!string.IsNullOrEmpty(railwayDbUrl))
{
    // Преобразуем URL формата Railway в строку подключения Npgsql
    var uri = new Uri(railwayDbUrl);
    var userInfo = uri.UserInfo.Split(':');
    
    var updatedConnectionString = $"Host={uri.Host};" +
                                 $"Database={uri.AbsolutePath.TrimStart('/')};" +
                                 $"Username={userInfo[0]};" +
                                 $"Password={userInfo[1]};" +
                                 $"Port={uri.Port};" +
                                 "SSL Mode=Require;Trust Server Certificate=true;";
    
    builder.Configuration["ConnectionStrings:DefaultConnection"] = updatedConnectionString;
}

// ========== НАСТРОЙКА СЕРВИСОВ ==========
// Контроллеры с JSON настройками
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.WriteIndented = builder.Environment.IsDevelopment();
    });

builder.Services.AddEndpointsApiExplorer();

// Настройка Swagger
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
            Email = "support@metaplatforme.ru"
        }
    });
    
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter JWT Bearer token",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
        }
    };
    
    c.AddSecurityDefinition("Bearer", securityScheme);
    
    // Добавляем security requirement для защищенных эндпоинтов
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// ========== БАЗА ДАННЫХ ==========
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (!string.IsNullOrEmpty(connectionString))
{
    builder.Services.AddDbContext<MetaplatformeContext>(options =>
        options.UseNpgsql(connectionString));
}

// ========== JWT АУТЕНТИФИКАЦИЯ ==========
// Получаем JWT секрет из переменных окружения или конфигурации
var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") ?? 
                builder.Configuration["Jwt:Secret"];
var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? 
                builder.Configuration["Jwt:Issuer"];
var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? 
                  builder.Configuration["Jwt:Audience"];

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
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };
    });
}

builder.Services.AddAuthorization();

// ========== РЕГИСТРАЦИЯ СЕРВИСОВ ==========
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPlaceService, PlaceService>();
builder.Services.AddScoped<IApplicationService, ApplicationService>();
builder.Services.AddScoped<IEventsService, EventsService>();
builder.Services.AddScoped<IStatusService, StatusService>();
builder.Services.AddScoped<EventsTypeController>();
builder.Services.AddHttpContextAccessor();

// ========== CORS ==========
builder.Services.AddCors(options =>
{
    // Политика для разработки
    options.AddPolicy("DevelopmentPolicy",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
    
    // Политика для продакшена
    options.AddPolicy("ProductionPolicy",
        builder =>
        {
            builder.WithOrigins(
                    "https://your-netlify-site.netlify.app", // Ваш фронтенд на Netlify
                    "https://metaplatforme.ru",              // Продакшен домен
                    "http://localhost:3000",                 // Локальный фронтенд
                    "http://localhost:5173"                  // Vite/React
                )
                   .AllowAnyMethod()
                   .AllowAnyHeader()
                   .AllowCredentials();
        });
});

// ========== СОЗДАНИЕ ПРИЛОЖЕНИЯ ==========
var app = builder.Build();

// ========== КОНФИГУРАЦИЯ ПАЙПЛАЙНА ==========
// Swagger только для разработки
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "MetaPl API v1");
        c.RoutePrefix = "swagger"; // Доступ по /swagger
    });
    app.UseCors("DevelopmentPolicy");
}
else
{
    app.UseCors("ProductionPolicy");
    // Health check для Railway
    app.MapGet("/", () => "MetaPl API is running");
    app.MapGet("/health", () => Results.Ok(new { 
        status = "Healthy", 
        timestamp = DateTime.UtcNow,
        environment = app.Environment.EnvironmentName 
    }));
}

// Принудительное HTTPS только если не в разработке
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Запуск приложения
app.Run();