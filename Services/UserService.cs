using MetaPlApi.Data.Entities;
using MetaPlApi.Models.DTOs.Requests;
using MetaPlApi.Models.DTOs.Responses;
using Microsoft.EntityFrameworkCore;

namespace MetaPlApi.Services
{
    public class UserService : IUserService
    {
        private readonly MetaplatformeContext _context;

        public UserService(MetaplatformeContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<UserResponse>> GetUserByIdAsync(int id)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.Id == id);

                if (user == null)
                {
                    return ApiResponse<UserResponse>.ErrorResponse("Пользователь не найден");
                }

                var response = new UserResponse
                {
                    Id = user.Id,
                    Login = user.Login,
                    Role = user.Role?.Name ?? "Пользователь",
                    RoleId = user.RoleId,
                    CreatedAt = user.CreatedAt
                };

                return ApiResponse<UserResponse>.SuccessResponse(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<UserResponse>.ErrorResponse($"Ошибка при получении пользователя: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<UserResponse>>> GetAllUsersAsync(int page = 1, int pageSize = 20)
        {
            try
            {
                // Простой тестовый ответ
                var testData = new List<UserResponse>
                {
                    new UserResponse { Id = 1, Login = "admin", Role = "Администратор", RoleId = 1 },
                    new UserResponse { Id = 2, Login = "manager", Role = "Менеджер", RoleId = 2 },
                    new UserResponse { Id = 3, Login = "user", Role = "Пользователь", RoleId = 3 }
                };

                return ApiResponse<List<UserResponse>>.SuccessResponse(testData);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<UserResponse>>.ErrorResponse($"Ошибка при получении пользователей: {ex.Message}");
            }
        }

        public async Task<ApiResponse<UserResponse>> UpdateUserAsync(int id, UpdateProfileRequest request)
        {
            return ApiResponse<UserResponse>.SuccessResponse(
                new UserResponse { Id = id, Login = "UpdatedUser", Role = "User" },
                "Метод временно отключен"
            );
        }

        public async Task<ApiResponse<bool>> DeleteUserAsync(int id)
        {
            return ApiResponse<bool>.SuccessResponse(true, "Метод временно отключен");
        }

        public async Task<ApiResponse<List<UserResponse>>> GetUsersByRoleAsync(int roleId)
        {
            try
            {
                var testData = new List<UserResponse>
                {
                    new UserResponse { Id = 1, Login = $"user_for_role_{roleId}", Role = "Пользователь", RoleId = roleId }
                };

                return ApiResponse<List<UserResponse>>.SuccessResponse(testData);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<UserResponse>>.ErrorResponse($"Ошибка при получении пользователей: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<UserResponse>>> SearchUsersAsync(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return ApiResponse<List<UserResponse>>.ErrorResponse("Поисковый запрос не может быть пустым");
                }

                var testData = new List<UserResponse>
                {
                    new UserResponse { Id = 1, Login = searchTerm, Role = "Пользователь", RoleId = 3 }
                };

                return ApiResponse<List<UserResponse>>.SuccessResponse(testData);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<UserResponse>>.ErrorResponse($"Ошибка при поиске пользователей: {ex.Message}");
            }
        }
    }
}
