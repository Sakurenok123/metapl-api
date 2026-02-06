using MetaPlApi.Data.Entities;
using MetaPlApi.Models.DTOs.Responses;
using Microsoft.EntityFrameworkCore;

namespace MetaPlApi.Services
{
    public class EventsService : IEventsService
    {
        private readonly MetaplatformeContext _context;

        public EventsService(MetaplatformeContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<List<EventResponse>>> GetAllEventsAsync()
        {
            try
            {
                var events = await _context.Events
                    .Include(e => e.EventType)
                    .OrderBy(e => e.Name)
                    .ToListAsync();

                var response = events.Select(e => new EventResponse
                {
                    Id = e.Id,
                    Name = e.Name,
                    EventTypeId = e.EventTypeId,
                    EventTypeName = e.EventType.NameEventsType
                }).ToList();

                return ApiResponse<List<EventResponse>>.SuccessResponse(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<EventResponse>>.ErrorResponse($"Ошибка при получении мероприятий: {ex.Message}");
            }
        }

        public async Task<ApiResponse<EventResponse>> GetEventByIdAsync(int id)
        {
            try
            {
                var eventItem = await _context.Events
                    .Include(e => e.EventType)
                    .FirstOrDefaultAsync(e => e.Id == id);

                if (eventItem == null)
                {
                    return ApiResponse<EventResponse>.ErrorResponse("Мероприятие не найдено");
                }

                var response = new EventResponse
                {
                    Id = eventItem.Id,
                    Name = eventItem.Name,
                    EventTypeId = eventItem.EventTypeId,
                    EventTypeName = eventItem.EventType.NameEventsType
                };

                return ApiResponse<EventResponse>.SuccessResponse(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<EventResponse>.ErrorResponse($"Ошибка при получении мероприятия: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<EventResponse>>> GetEventsByTypeAsync(int typeId)
        {
            try
            {
                var events = await _context.Events
                    .Include(e => e.EventType)
                    .Where(e => e.EventTypeId == typeId)
                    .OrderBy(e => e.Name)
                    .ToListAsync();

                var response = events.Select(e => new EventResponse
                {
                    Id = e.Id,
                    Name = e.Name,
                    EventTypeId = e.EventTypeId,
                    EventTypeName = e.EventType.NameEventsType
                }).ToList();

                return ApiResponse<List<EventResponse>>.SuccessResponse(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<EventResponse>>.ErrorResponse($"Ошибка при получении мероприятий по типу: {ex.Message}");
            }
        }
    }
}