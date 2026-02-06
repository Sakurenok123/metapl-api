using MetaPlApi.Models.DTOs.Responses;

namespace MetaPlApi.Services
{
    public interface IStatusService
    {
        Task<ApiResponse<List<StatusResponse>>> GetAllStatusesAsync();
        Task<ApiResponse<StatusResponse>> GetStatusByIdAsync(int id);
    }

    public class StatusResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}