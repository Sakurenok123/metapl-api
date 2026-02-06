using MetaPlApi.Models.DTOs.Requests;
using MetaPlApi.Models.DTOs.Responses;

namespace MetaPlApi.Services
{
    public interface IApplicationService
    {
        Task<ApiResponse<ApplicationResponse>> GetApplicationByIdAsync(int id);
        Task<ApiResponse<List<ApplicationResponse>>> GetAllApplicationsAsync(ApplicationFilterRequest filter);
        Task<ApiResponse<ApplicationResponse>> CreateApplicationAsync(CreateApplicationRequest request);
        Task<ApiResponse<ApplicationResponse>> UpdateApplicationAsync(int id, UpdateApplicationRequest request);
        Task<ApiResponse<bool>> DeleteApplicationAsync(int id);
        Task<ApiResponse<List<ApplicationResponse>>> GetApplicationsByUserIdAsync(int userId);
        Task<ApiResponse<ApplicationStatsResponse>> GetApplicationStatsAsync();
    }
}
