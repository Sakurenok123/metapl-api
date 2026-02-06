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
                    Role = user.Role.Name,
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
                var query = _context.Users
                    .Include(u => u.Role)
                    .AsQueryable();

                var totalCount = await query.CountAsync();
                var users = await query
                    .OrderBy(u => u.Id)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var response = users.Select(u => new UserResponse
                {
                    Id = u.Id,
                    Login = u.Login,
                    Role = u.Role.Name,
                    RoleId = u.RoleId,
                    CreatedAt = u.CreatedAt
                }).ToList();

                var apiResponse = ApiResponse<List<UserResponse>>.SuccessResponse(response);
                apiResponse.Pagination = new PaginationInfo
                {
                    Page = page,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                };

                return apiResponse;
            }
            catch (Exception ex)
            {
                return ApiResponse<List<UserResponse>>.ErrorResponse($"Ошибка при получении пользователей: {ex.Message}");
            }
        }

        public async Task<ApiResponse<UserResponse>> UpdateUserAsync(int id, UpdateProfileRequest request)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);

                if (user == null)
                {
                    return ApiResponse<UserResponse>.ErrorResponse("Пользователь не найден");
                }

                // Проверка логина на уникальность
                if (!string.IsNullOrEmpty(request.Login) && request.Login != user.Login)
                {
                    var existingUser = await _context.Users
                        .FirstOrDefaultAsync(u => u.Login == request.Login && u.Id != id);

                    if (existingUser != null)
                    {
                        return ApiResponse<UserResponse>.ErrorResponse("Пользователь с таким логином уже существует");
                    }

                    user.Login = request.Login;
                }

                if (request.RoleId.HasValue)
                {
                    var role = await _context.Roles.FindAsync(request.RoleId.Value);

                    if (role == null)
                    {
                        return ApiResponse<UserResponse>.ErrorResponse("Указанная роль не существует");
                    }

                    user.RoleId = request.RoleId.Value;
                }

                await _context.SaveChangesAsync();

                // Получаем обновленные данные с ролью
                var updatedUser = await _context.Users
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.Id == id);

                var response = new UserResponse
                {
                    Id = updatedUser.Id,
                    Login = updatedUser.Login,
                    Role = updatedUser.Role.Name,
                    RoleId = updatedUser.RoleId
                };

                return ApiResponse<UserResponse>.SuccessResponse(response, "Пользователь успешно обновлен");
            }
            catch (Exception ex)
            {
                return ApiResponse<UserResponse>.ErrorResponse($"Ошибка при обновлении пользователя: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> DeleteUserAsync(int id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);

                if (user == null)
                {
                    return ApiResponse<bool>.ErrorResponse("Пользователь не найден");
                }

                _context.Users.Remove(user);
                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResponse(true, "Пользователь успешно удален");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResponse($"Ошибка при удалении пользователя: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<UserResponse>>> GetUsersByRoleAsync(int roleId)
        {
            try
            {
                var users = await _context.Users
                    .Include(u => u.Role)
                    .Where(u => u.RoleId == roleId)
                    .OrderBy(u => u.Login)
                    .ToListAsync();

                var response = users.Select(u => new UserResponse
                {
                    Id = u.Id,
                    Login = u.Login,
                    Role = u.Role.Name,
                    RoleId = u.RoleId,
                    CreatedAt = u.CreatedAt
                }).ToList();

                return ApiResponse<List<UserResponse>>.SuccessResponse(response);
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

                var users = await _context.Users
                    .Include(u => u.Role)
                    .Where(u => u.Login.Contains(searchTerm) || u.Role.Name.Contains(searchTerm))
                    .OrderBy(u => u.Login)
                    .Take(50)
                    .ToListAsync();

                var response = users.Select(u => new UserResponse
                {
                    Id = u.Id,
                    Login = u.Login,
                    Role = u.Role.Name,
                    RoleId = u.RoleId,
                    CreatedAt = u.CreatedAt
                }).ToList();

                return ApiResponse<List<UserResponse>>.SuccessResponse(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<UserResponse>>.ErrorResponse($"Ошибка при поиске пользователей: {ex.Message}");
            }
        }
    }
}