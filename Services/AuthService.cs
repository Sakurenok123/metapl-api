using MetaPlApi.Data.Entities;
using MetaPlApi.Helpers;
using MetaPlApi.Models.DTOs.Requests;
using MetaPlApi.Models.DTOs.Responses;
using Microsoft.EntityFrameworkCore;

namespace MetaPlApi.Services
{
    public class AuthService : IAuthService
    {
        private readonly MetaplatformeContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(MetaplatformeContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.Login == request.Login);

                if (user == null)
                {
                    return ApiResponse<AuthResponse>.ErrorResponse("Пользователь не найден");
                }

                // Проверка пароля (в вашей БД пароль хранится в открытом виде)
                if (user.Password != request.Password)
                {
                    return ApiResponse<AuthResponse>.ErrorResponse("Неверный пароль");
                }

                // Генерация токена
                var token = TokenHelper.GenerateJwtToken(user.Id, user.Role.Name, _configuration);
                var response = new AuthResponse
                {
                    UserId = user.Id,
                    Login = user.Login,
                    Role = user.Role.Name,
                    Token = token,
                    TokenExpiry = DateTime.UtcNow.AddHours(2)
                };

                return ApiResponse<AuthResponse>.SuccessResponse(response, "Вход выполнен успешно");
            }
            catch (Exception ex)
            {
                return ApiResponse<AuthResponse>.ErrorResponse($"Ошибка при входе: {ex.Message}");
            }
        }

        public async Task<ApiResponse<AuthResponse>> RegisterAsync(RegisterRequest request)
{
    try
    {
        // Проверка существующего пользователя
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Login == request.Login);

        if (existingUser != null)
        {
            return ApiResponse<AuthResponse>.ErrorResponse("Пользователь с таким логином уже существует");
        }

        // Устанавливаем роль по умолчанию, если не указана
        var roleId = request.RoleId;
        if (roleId <= 0)
        {
            roleId = 3; // Роль "Пользователь"
        }

        // Проверка существования роли
        var role = await _context.Roles.FindAsync(roleId);
        if (role == null)
        {
            return ApiResponse<AuthResponse>.ErrorResponse("Указанная роль не существует");
        }

        // Создание пользователя
        var user = new User
        {
            Login = request.Login,
            Password = request.Password,
            RoleId = roleId
        };

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Генерация токена
        var token = TokenHelper.GenerateJwtToken(user.Id, role.Name, _configuration);
        var response = new AuthResponse
        {
            UserId = user.Id,
            Login = user.Login,
            Role = role.Name,
            Token = token,
            TokenExpiry = DateTime.UtcNow.AddHours(2)
        };

        return ApiResponse<AuthResponse>.SuccessResponse(response, "Регистрация выполнена успешно");
    }
    catch (Exception ex)
    {
        return ApiResponse<AuthResponse>.ErrorResponse($"Ошибка при регистрации: {ex.Message}");
    }
}

        public async Task<ApiResponse<bool>> ChangePasswordAsync(int userId, ChangePasswordRequest request)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);

                if (user == null)
                {
                    return ApiResponse<bool>.ErrorResponse("Пользователь не найден");
                }

                // Проверка старого пароля
                if (user.Password != request.OldPassword)
                {
                    return ApiResponse<bool>.ErrorResponse("Неверный старый пароль");
                }

                // Обновление пароля
                user.Password = request.NewPassword;
                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResponse(true, "Пароль успешно изменен");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResponse($"Ошибка при изменении пароля: {ex.Message}");
            }
        }
    }
}