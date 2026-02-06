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

    public class ApplicationResponse
    {
        public int Id { get; set; }
        public string Status { get; set; } = string.Empty;
        public int StatusId { get; set; }
        public int EventId { get; set; }
        public int EventTypeId { get; set; }
        public int PlaceId { get; set; }
        public string PlaceName { get; set; } = string.Empty;
        public string PlaceAddress { get; set; } = string.Empty;
        public List<string> ServiceNames { get; set; } = new List<string>();
        public List<int> ServiceIds { get; set; } = new List<int>();
        public DateTime? DateCreate { get; set; }
        public DateTime? DateUpdate { get; set; }
        public string ContactPhone { get; set; } = string.Empty;
        public int? GuestCount { get; set; }
        public DateOnly? EventDate { get; set; }
        public TimeOnly? EventTime { get; set; }
        public int? Duration { get; set; }
        public string AdditionalInfo { get; set; } = string.Empty;
        public string EventName { get; set; } = string.Empty;
        public string EventTypeName { get; set; } = string.Empty;
        public UserInfo? User { get; set; }
    }

    public class ApplicationStatsResponse
    {
        public int TotalApplications { get; set; }
        public int ApplicationsToday { get; set; }
        public Dictionary<string, int> ApplicationsByStatus { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> ApplicationsByDay { get; set; } = new Dictionary<string, int>();
    }
    
    public class UserInfo
    {
        public int Id { get; set; }
        public string Login { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}