using MetaPlApi.Models.DTOs.Responses;

namespace MetaPlApi.Services
{
    public interface IEventsService
    {
        Task<ApiResponse<List<EventResponse>>> GetAllEventsAsync();
        Task<ApiResponse<EventResponse>> GetEventByIdAsync(int id);
        Task<ApiResponse<List<EventResponse>>> GetEventsByTypeAsync(int typeId);
    }

    public class EventResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int EventTypeId { get; set; }
        public string EventTypeName { get; set; } = string.Empty;
    }
}