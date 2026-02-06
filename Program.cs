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
// Получаем порт из переменной окружения Railway
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";

// Конфигурируем Kestrel для работы на Railway
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(int.Parse(port));
});

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

// ========== ОСТАЛЬНОЙ КОД (без изменений) ==========
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.WriteIndented = true;
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
});

// Настройка базы данных
builder.Services.AddDbContext<MetaplatformeContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Настройка JWT аутентификации
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secret = jwtSettings["Secret"];

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
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret))
    };
});

builder.Services.AddAuthorization();

// Регистрация сервисов
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IPlaceService, PlaceService>();
builder.Services.AddScoped<IApplicationService, ApplicationService>();
builder.Services.AddScoped<IEventsService, EventsService>();
builder.Services.AddScoped<IStatusService, StatusService>();
builder.Services.AddHttpContextAccessor();

// Настройка CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
});

var app = builder.Build();

// Конфигурация конвейера HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "MetaPl API v1");
        c.RoutePrefix = "swagger"; // Доступ по /swagger
    });
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// ========== HEALTH CHECK ДЛЯ RAILWAY ==========
app.MapGet("/", () => "MetaPl API is running");
app.MapGet("/health", () => Results.Ok(new { 
    status = "Healthy", 
    timestamp = DateTime.UtcNow,
    environment = app.Environment.EnvironmentName 
}));

app.Run();
