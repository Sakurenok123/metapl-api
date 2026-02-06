using MetaPlApi.Data.Entities;
using MetaPlApi.Models.DTOs.Requests;
using MetaPlApi.Models.DTOs.Responses;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace MetaPlApi.Services
{
    public class ApplicationService : IApplicationService
    {
        private readonly MetaplatformeContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApplicationService(MetaplatformeContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ApiResponse<ApplicationResponse>> GetApplicationByIdAsync(int id)
        {
            try
            {
                var application = await _context.Applications
                    .Include(a => a.Status)
                    .Include(a => a.Event)
                    .ThenInclude(e => e.EventType)
                    .Include(a => a.Place)
                    .ThenInclude(p => p.Address)
                    .Include(a => a.ServicesApplications)
                    .ThenInclude(sa => sa.Service)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (application == null)
                {
                    return ApiResponse<ApplicationResponse>.ErrorResponse("Заявка не найдена");
                }

                var response = new ApplicationResponse
                {
                    Id = application.Id,
                    Status = application.Status?.Name ?? "Не указан",
                    StatusId = application.StatusId,
                    EventId = application.EventId,
                    PlaceId = application.PlaceId,
                    PlaceName = application.Place?.Name ?? "Не указано",
                    PlaceAddress = application.Place?.Address != null ?
                        $"{application.Place.Address.City}, {application.Place.Address.Street}, {application.Place.Address.House}" :
                        "Адрес не указан",
                    ServiceNames = application.ServicesApplications
                        .Select(sa => sa.Service?.Name ?? "Не указано")
                        .ToList(),
                    ServiceIds = application.ServicesApplications
                        .Select(sa => sa.ServiceId)
                        .ToList(),
                    DateCreate = application.DateCreate,
                    DateUpdate = application.DateUpdate,
                    ContactPhone = application.ContactPhone ?? string.Empty,
                    GuestCount = application.GuestCount,
                    EventDate = application.EventDate,
                    EventTime = application.EventTime,
                    Duration = application.Duration,
                    AdditionalInfo = application.AdditionalInfo ?? string.Empty,
                    EventName = application.Event?.Name ?? "Не указано",
                    EventTypeName = application.Event?.EventType?.NameEventsType ?? "Не указано"
                };

                return ApiResponse<ApplicationResponse>.SuccessResponse(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<ApplicationResponse>.ErrorResponse($"Ошибка при получении заявки: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<ApplicationResponse>>> GetAllApplicationsAsync(ApplicationFilterRequest filter)
        {
            try
            {
                var query = _context.Applications
                    .Include(a => a.Status)
                    .Include(a => a.Event)
                    .ThenInclude(e => e.EventType)
                    .Include(a => a.Place)
                    .ThenInclude(p => p.Address)
                    .Include(a => a.ServicesApplications)
                    .ThenInclude(sa => sa.Service)
                    .Include(a => a.User)
                    .AsQueryable();

                if (filter.StatusId.HasValue)
                {
                    query = query.Where(a => a.StatusId == filter.StatusId.Value);
                }

                // Используем filter.SearchQuery вместо UserLogin
                if (!string.IsNullOrEmpty(filter.SearchQuery))
                {
                    query = query.Where(a => 
                        a.User.Login.Contains(filter.SearchQuery) ||
                        a.Event.Name.Contains(filter.SearchQuery) ||
                        a.Place.Name.Contains(filter.SearchQuery));
                }

                if (filter.DateFrom.HasValue)
                {
                    query = query.Where(a => a.DateCreate >= filter.DateFrom.Value);
                }

                if (filter.DateTo.HasValue)
                {
                    query = query.Where(a => a.DateCreate <= filter.DateTo.Value);
                }

                var totalCount = await query.CountAsync();
                var applications = await query
                    .OrderByDescending(a => a.DateCreate)
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToListAsync();

                var response = applications.Select(a => new ApplicationResponse
                {
                    Id = a.Id,
                    Status = a.Status?.Name ?? "Не указан",
                    StatusId = a.StatusId,
                    EventId = a.EventId,
                    PlaceId = a.PlaceId,
                    PlaceName = a.Place?.Name ?? "Не указано",
                    PlaceAddress = a.Place?.Address != null ?
                        $"{a.Place.Address.City}, {a.Place.Address.Street}, {a.Place.Address.House}" :
                        "Адрес не указан",
                    ServiceNames = a.ServicesApplications
                        .Select(sa => sa.Service?.Name ?? "Не указано")
                        .ToList(),
                    ServiceIds = a.ServicesApplications
                        .Select(sa => sa.ServiceId)
                        .ToList(),
                    DateCreate = a.DateCreate,
                    DateUpdate = a.DateUpdate,
                    ContactPhone = a.ContactPhone ?? string.Empty,
                    GuestCount = a.GuestCount,
                    EventDate = a.EventDate,
                    EventTime = a.EventTime,
                    Duration = a.Duration,
                    AdditionalInfo = a.AdditionalInfo ?? string.Empty,
                    EventName = a.Event?.Name ?? "Не указано",
                    EventTypeName = a.Event?.EventType?.NameEventsType ?? "Не указано",
                    User = a.User != null ? new UserInfo
                    {
                        Id = a.User.Id,
                        Login = a.User.Login,
                        Role = a.User.Role?.Name ?? "Пользователь"
                    } : null
                }).ToList();

                var apiResponse = ApiResponse<List<ApplicationResponse>>.SuccessResponse(response);
                apiResponse.Pagination = new PaginationInfo
                {
                    Page = filter.Page,
                    PageSize = filter.PageSize,
                    TotalCount = totalCount,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize)
                };

                return apiResponse;
            }
            catch (Exception ex)
            {
                return ApiResponse<List<ApplicationResponse>>.ErrorResponse($"Ошибка при получении заявок: {ex.Message}");
            }
        }

        public async Task<ApiResponse<ApplicationResponse>> CreateApplicationAsync(CreateApplicationRequest request)
        {
            try
            {
                var status = await _context.Statuses.FindAsync(1); // Всегда "Новая" при создании
                if (status == null)
                {
                    return ApiResponse<ApplicationResponse>.ErrorResponse("Статус 'Новая' не найден");
                }

                var eventType = await _context.EventsTypes.FindAsync(request.EventTypeId);
                if (eventType == null)
                {
                    return ApiResponse<ApplicationResponse>.ErrorResponse("Указанный тип мероприятия не существует");
                }

                var place = await _context.Places.FindAsync(request.PlaceId);
                if (place == null)
                {
                    return ApiResponse<ApplicationResponse>.ErrorResponse("Указанное место не существует");
                }

                if (request.ServiceIds == null || !request.ServiceIds.Any())
                {
                    return ApiResponse<ApplicationResponse>.ErrorResponse("Необходимо указать хотя бы одну услугу");
                }

                foreach (var serviceId in request.ServiceIds)
                {
                    var service = await _context.Services.FindAsync(serviceId);
                    if (service == null)
                    {
                        return ApiResponse<ApplicationResponse>.ErrorResponse($"Услуга с ID {serviceId} не существует");
                    }
                }

                var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);
                int userId = 0;
                
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int parsedUserId))
                {
                    userId = parsedUserId;
                    
                    var user = await _context.Users.FindAsync(userId);
                    if (user == null)
                    {
                        return ApiResponse<ApplicationResponse>.ErrorResponse("Пользователь не найден");
                    }
                }
                else
                {
                    return ApiResponse<ApplicationResponse>.ErrorResponse("Пользователь не авторизован");
                }

                var newEvent = new Event
                {
                    Name = request.EventName,
                    EventTypeId = request.EventTypeId
                };

                await _context.Events.AddAsync(newEvent);
                await _context.SaveChangesAsync();

                var application = new Application
                {
                    StatusId = 1, // Всегда "Новая"
                    EventId = newEvent.Id,
                    PlaceId = request.PlaceId,
                    UserId = userId,
                    DateCreate = DateTime.Now,
                    DateUpdate = DateTime.Now,
                    ContactPhone = request.ContactPhone?.Trim() ?? string.Empty,
                    GuestCount = request.GuestCount,
                    EventDate = request.EventDate,
                    EventTime = request.EventTime,
                    Duration = request.Duration,
                    AdditionalInfo = request.AdditionalInfo?.Trim() ?? string.Empty
                };

                await _context.Applications.AddAsync(application);
                await _context.SaveChangesAsync();

                foreach (var serviceId in request.ServiceIds)
                {
                    await _context.ServicesApplications.AddAsync(new ServicesApplication
                    {
                        ApplicationId = application.Id,
                        ServiceId = serviceId
                    });
                }

                await _context.SaveChangesAsync();

                var createdApplication = await GetApplicationByIdAsync(application.Id);
                return createdApplication;
            }
            catch (Exception ex)
            {
                return ApiResponse<ApplicationResponse>.ErrorResponse($"Ошибка при создании заявки: {ex.Message}");
            }
        }

        public async Task<ApiResponse<ApplicationResponse>> UpdateApplicationAsync(int id, UpdateApplicationRequest request)
        {
            try
            {
                var application = await _context.Applications
                    .Include(a => a.ServicesApplications)
                    .Include(a => a.Event)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (application == null)
                {
                    return ApiResponse<ApplicationResponse>.ErrorResponse("Заявка не найдена");
                }

                if (request.StatusId.HasValue)
                {
                    var status = await _context.Statuses.FindAsync(request.StatusId.Value);
                    if (status == null)
                    {
                        return ApiResponse<ApplicationResponse>.ErrorResponse("Указанный статус не существует");
                    }
                    application.StatusId = request.StatusId.Value;
                }

                if (!string.IsNullOrEmpty(request.EventName) || request.EventTypeId.HasValue)
                {
                    if (application.Event == null)
                    {
                        application.Event = new Event();
                    }
                    
                    if (!string.IsNullOrEmpty(request.EventName))
                    {
                        application.Event.Name = request.EventName;
                    }
                    
                    if (request.EventTypeId.HasValue)
                    {
                        var eventType = await _context.EventsTypes.FindAsync(request.EventTypeId.Value);
                        if (eventType == null)
                        {
                            return ApiResponse<ApplicationResponse>.ErrorResponse("Указанный тип мероприятия не существует");
                        }
                        application.Event.EventTypeId = request.EventTypeId.Value;
                    }
                }

                if (request.PlaceId.HasValue)
                {
                    var place = await _context.Places.FindAsync(request.PlaceId.Value);
                    if (place == null)
                    {
                        return ApiResponse<ApplicationResponse>.ErrorResponse("Указанное место не существует");
                    }
                    application.PlaceId = request.PlaceId.Value;
                }

                if (request.ServiceIds != null && request.ServiceIds.Any())
                {
                    _context.ServicesApplications.RemoveRange(application.ServicesApplications);
                    
                    foreach (var serviceId in request.ServiceIds)
                    {
                        var service = await _context.Services.FindAsync(serviceId);
                        if (service == null)
                        {
                            return ApiResponse<ApplicationResponse>.ErrorResponse($"Услуга с ID {serviceId} не существует");
                        }
                        
                        await _context.ServicesApplications.AddAsync(new ServicesApplication
                        {
                            ApplicationId = id,
                            ServiceId = serviceId
                        });
                    }
                }

                if (!string.IsNullOrEmpty(request.ContactPhone))
                {
                    application.ContactPhone = request.ContactPhone;
                }

                if (request.GuestCount.HasValue)
                {
                    application.GuestCount = request.GuestCount.Value;
                }

                if (request.EventDate.HasValue)
                {
                    application.EventDate = request.EventDate.Value;
                }

                if (request.EventTime.HasValue)
                {
                    application.EventTime = request.EventTime.Value;
                }

                if (request.Duration.HasValue)
                {
                    application.Duration = request.Duration.Value;
                }

                if (request.AdditionalInfo != null)
                {
                    application.AdditionalInfo = request.AdditionalInfo;
                }

                application.DateUpdate = DateTime.Now;
                await _context.SaveChangesAsync();

                var updatedApplication = await GetApplicationByIdAsync(id);
                return updatedApplication;
            }
            catch (Exception ex)
            {
                return ApiResponse<ApplicationResponse>.ErrorResponse($"Ошибка при обновлении заявки: {ex.Message}");
            }
        }

        public async Task<ApiResponse<bool>> DeleteApplicationAsync(int id)
        {
            try
            {
                var application = await _context.Applications
                    .Include(a => a.ServicesApplications)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (application == null)
                {
                    return ApiResponse<bool>.ErrorResponse("Заявка не найдена");
                }

                // Удаляем связанные записи из junction-таблиц
                _context.ServicesApplications.RemoveRange(application.ServicesApplications);

                _context.Applications.Remove(application);
                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResponse(true, "Заявка успешно удалена");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.ErrorResponse($"Ошибка при удалении заявки: {ex.Message}");
            }
        }

        public async Task<ApiResponse<List<ApplicationResponse>>> GetApplicationsByUserIdAsync(int userId)
        {
            try
            {
                var applications = await _context.Applications
                    .Where(a => a.UserId == userId)
                    .Include(a => a.Status)
                    .Include(a => a.Event)
                    .ThenInclude(e => e.EventType)
                    .Include(a => a.Place)
                    .ThenInclude(p => p.Address)
                    .Include(a => a.ServicesApplications)
                    .ThenInclude(sa => sa.Service)
                    .OrderByDescending(a => a.DateCreate)
                    .ToListAsync();

                var response = applications.Select(a => new ApplicationResponse
                {
                    Id = a.Id,
                    Status = a.Status?.Name ?? "Не указан",
                    StatusId = a.StatusId,
                    EventId = a.EventId,
                    PlaceId = a.PlaceId,
                    PlaceName = a.Place?.Name ?? "Не указано",
                    PlaceAddress = a.Place?.Address != null ?
                        $"{a.Place.Address.City}, {a.Place.Address.Street}, {a.Place.Address.House}" :
                        "Адрес не указан",
                    ServiceNames = a.ServicesApplications
                        .Select(sa => sa.Service?.Name ?? "Не указано")
                        .ToList(),
                    ServiceIds = a.ServicesApplications
                        .Select(sa => sa.ServiceId)
                        .ToList(),
                    DateCreate = a.DateCreate,
                    DateUpdate = a.DateUpdate,
                    ContactPhone = a.ContactPhone ?? string.Empty,
                    GuestCount = a.GuestCount,
                    EventDate = a.EventDate,
                    EventTime = a.EventTime,
                    Duration = a.Duration,
                    AdditionalInfo = a.AdditionalInfo ?? string.Empty,
                    EventName = a.Event?.Name ?? "Не указано",
                    EventTypeName = a.Event?.EventType?.NameEventsType ?? "Не указано"
                }).ToList();

                return ApiResponse<List<ApplicationResponse>>.SuccessResponse(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<List<ApplicationResponse>>.ErrorResponse($"Ошибка при получении заявок пользователя: {ex.Message}");
            }
        }

        public async Task<ApiResponse<ApplicationStatsResponse>> GetApplicationStatsAsync()
        {
            try
            {
                var totalApplications = await _context.Applications.CountAsync();
                var applicationsToday = await _context.Applications
                    .Where(a => a.DateCreate != null && a.DateCreate.Value.Date == DateTime.Today)
                    .CountAsync();
                var applicationsByStatus = await _context.Applications
                    .Include(a => a.Status)
                    .GroupBy(a => a.Status.Name)
                    .Select(g => new { Status = g.Key, Count = g.Count() })
                    .ToDictionaryAsync(x => x.Status, x => x.Count);
                var applicationsByDay = await _context.Applications
                    .Where(a => a.DateCreate != null && a.DateCreate.Value.Date >= DateTime.Today.AddDays(-30))
                    .GroupBy(a => a.DateCreate.Value.Date)
                    .Select(g => new { Date = g.Key.ToString("yyyy-MM-dd"), Count = g.Count() })
                    .OrderBy(x => x.Date)
                    .ToDictionaryAsync(x => x.Date, x => x.Count);

                var response = new ApplicationStatsResponse
                {
                    TotalApplications = totalApplications,
                    ApplicationsToday = applicationsToday,
                    ApplicationsByStatus = applicationsByStatus,
                    ApplicationsByDay = applicationsByDay
                };

                return ApiResponse<ApplicationStatsResponse>.SuccessResponse(response);
            }
            catch (Exception ex)
            {
                return ApiResponse<ApplicationStatsResponse>.ErrorResponse($"Ошибка при получении статистики: {ex.Message}");
            }
        }
    }
}