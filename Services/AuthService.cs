using MetaPlApi.Data.Entities;
using MetaPlApi.Models.DTOs.Requests;
using MetaPlApi.Models.DTOs.Responses;
using Microsoft.EntityFrameworkCore;

namespace MetaPlApi.Services
{
    public class AuthService : IAuthService
    {
        private readonly MetaplatformeContext _context;

        public AuthService(MetaplatformeContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<AuthResponse>> LoginAsync(LoginRequest request)
        {
            try
            {
                // Простая проверка для тестирования
                if (request.Login == "admin" && request.Password == "admin123")
                {
                    var response = new AuthResponse
                    {
                        UserId = 1,
                        Login = "admin",
                        Role = "Admin",
                        Token = "test-token-123",
                        TokenExpiry = DateTime.UtcNow.AddHours(24)
                    };

                    return ApiResponse<AuthResponse>.SuccessResponse(response, "Вход выполнен успешно");
                }
                else if (request.Login == "user" && request.Password == "user123")
                {
                    var response = new AuthResponse
                    {
                        UserId = 2,
                        Login = "user",
                        Role = "User",
                        Token = "test-token-456",
                        TokenExpiry = DateTime.UtcNow.AddHours(24)
                    };

                    return ApiResponse<AuthResponse>.SuccessResponse(response, "Вход выполнен успешно");
                }

                return ApiResponse<AuthResponse>.ErrorResponse("Неверный логин или пароль");
            }
            catch (Exception ex)
            {
                return ApiResponse<AuthResponse>.ErrorResponse($"Ошибка при входе: {ex.Message}");
            }
        }

        public async Task<ApiResponse<AuthResponse>> RegisterAsync(RegisterRequest request)
        {
            return ApiResponse<AuthResponse>.SuccessResponse(new AuthResponse
            {
                UserId = 3,
                Login = request.Login,
                Role = "User",
                Token = "test-token-register",
                TokenExpiry = DateTime.UtcNow.AddHours(24)
            }, "Регистрация выполнена успешно");
        }
        
        public async Task<ApiResponse<bool>> ChangePasswordAsync(int userId, ChangePasswordRequest request)
        {
            return ApiResponse<bool>.SuccessResponse(true, "Пароль успешно изменен");
        }
    }
}
