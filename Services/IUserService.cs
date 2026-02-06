using MetaPlApi.Models.DTOs.Requests;
using MetaPlApi.Models.DTOs.Responses;

namespace MetaPlApi.Services
{
    public interface IUserService
    {
        Task<ApiResponse<UserResponse>> GetUserByIdAsync(int id);
        Task<ApiResponse<List<UserResponse>>> GetAllUsersAsync(int page = 1, int pageSize = 20);
        Task<ApiResponse<UserResponse>> UpdateUserAsync(int id, UpdateProfileRequest request);
        Task<ApiResponse<bool>> DeleteUserAsync(int id);
        Task<ApiResponse<List<UserResponse>>> GetUsersByRoleAsync(int roleId);
        Task<ApiResponse<List<UserResponse>>> SearchUsersAsync(string searchTerm);
    }
}