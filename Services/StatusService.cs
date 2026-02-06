using MetaPlApi.Data.Entities;
using MetaPlApi.Models.DTOs.Responses;
using Microsoft.EntityFrameworkCore;

namespace MetaPlApi.Services
{
    public class StatusService : IStatusService
    {
        private readonly MetaplatformeContext _context;

        public StatusService(MetaplatformeContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<List<StatusResponse>>> GetAllStatusesAsync()
        {
            try
            {
                var statuses = await _context.Statuses
                    .OrderBy(s => s.Name)
                    .ToListAsync();

                var response = statuses.Select(s => new StatusResponse
                {
                    Id = s.Id,
                    Name = s.Name
                }).ToList();

                return ApiResponse<List<StatusResponse>>.SuccessResponse(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<StatusResponse>>.ErrorResponse($"Ошибка при получении статусов: {ex.Message}");
            }
        }

        public async Task<ApiResponse<StatusResponse>> GetStatusByIdAsync(int id)
        {
            try
            {
                var status = await _context.Statuses.FindAsync(id);

                if (status == null)
                {
                    return ApiResponse<StatusResponse>.ErrorResponse("Статус не найден");
                }

                var response = new StatusResponse
                {
                    Id = status.Id,
                    Name = status.Name
                };

                return ApiResponse<StatusResponse>.SuccessResponse(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<StatusResponse>.ErrorResponse($"Ошибка при получении статуса: {ex.Message}");
            }
        }
    }
}