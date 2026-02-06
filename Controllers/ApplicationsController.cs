using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MetaPlApi.Data.Entities;
using MetaPlApi.Models.DTOs.Requests;
using MetaPlApi.Models.DTOs.Responses;
using System.Security.Claims;
using MetaPlApi.Services;

namespace MetaPlApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApplicationsController : ControllerBase
    {
        private readonly MetaplatformeContext _context;

        public ApplicationsController(MetaplatformeContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllApplications([FromQuery] ApplicationFilterRequest filter)
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
                    .ThenInclude(u => u.Role)
                    .AsQueryable();

                if (filter.StatusId.HasValue)
                {
                    query = query.Where(a => a.StatusId == filter.StatusId.Value);
                }

                if (filter.EventId.HasValue)
                {
                    query = query.Where(a => a.EventId == filter.EventId.Value);
                }

                if (filter.PlaceId.HasValue)
                {
                    query = query.Where(a => a.PlaceId == filter.PlaceId.Value);
                }

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
                    EventTypeId = a.Event?.EventTypeId ?? 0,
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

                return Ok(apiResponse);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse($"Ошибка при получении заявок: {ex.Message}"));
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetApplicationById(int id)
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
                    .Include(a => a.User)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (application == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResponse("Заявка не найдена"));
                }

                var response = new ApplicationResponse
                {
                    Id = application.Id,
                    Status = application.Status?.Name ?? "Не указан",
                    StatusId = application.StatusId,
                    EventId = application.EventId,
                    EventTypeId = application.Event?.EventTypeId ?? 0,
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
                    EventTypeName = application.Event?.EventType?.NameEventsType ?? "Не указано",
                    User = application.User != null ? new UserInfo
                    {
                        Id = application.User.Id,
                        Login = application.User.Login,
                        Role = application.User.Role?.Name ?? "Пользователь"
                    } : null
                };

                return Ok(ApiResponse<ApplicationResponse>.SuccessResponse(response));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse($"Ошибка при получении заявки: {ex.Message}"));
            }
        }

        // НОВЫЙ МЕТОД: Получить заявку с полными данными
        [HttpGet("{id}/full")]
        public async Task<IActionResult> GetApplicationWithFullDetails(int id)
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
                    .Include(a => a.User)
                    .ThenInclude(u => u.Role)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (application == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResponse("Заявка не найдена"));
                }

                var response = new
                {
                    application.Id,
                    application.StatusId,
                    Status = application.Status?.Name,
                    application.EventId,
                    Event = application.Event != null ? new
                    {
                        application.Event.Id,
                        application.Event.Name,
                        application.Event.EventTypeId,
                        EventType = application.Event.EventType?.NameEventsType
                    } : null,
                    application.PlaceId,
                    Place = application.Place != null ? new
                    {
                        application.Place.Id,
                        application.Place.Name,
                        Address = application.Place.Address != null ? new
                        {
                            application.Place.Address.Id,
                            application.Place.Address.City,
                            application.Place.Address.Street,
                            application.Place.Address.House,
                            FullAddress = $"{application.Place.Address.City}, {application.Place.Address.Street}, {application.Place.Address.House}"
                        } : null
                    } : null,
                    Services = application.ServicesApplications.Select(sa => new
                    {
                        sa.ServiceId,
                        ServiceName = sa.Service?.Name
                    }).ToList(),
                    ServiceIds = application.ServicesApplications.Select(sa => sa.ServiceId).ToList(),
                    application.DateCreate,
                    application.DateUpdate,
                    application.ContactPhone,
                    application.GuestCount,
                    application.EventDate,
                    application.EventTime,
                    application.Duration,
                    application.AdditionalInfo,
                    User = application.User != null ? new
                    {
                        application.User.Id,
                        application.User.Login,
                        Role = application.User.Role?.Name
                    } : null
                };

                return Ok(ApiResponse<object>.SuccessResponse(response));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse($"Ошибка при получении заявки: {ex.Message}"));
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateApplication([FromBody] CreateApplicationRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    
                    return BadRequest(ApiResponse<object>.ErrorResponse("Ошибка валидации", errors));
                }

                // Проверяем существование типа мероприятия
                var eventType = await _context.EventsTypes.FindAsync(request.EventTypeId);
                if (eventType == null)
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse("Указанный тип мероприятия не существует"));
                }

                // Проверяем существование площадки
                var place = await _context.Places
                    .Include(p => p.Address)
                    .FirstOrDefaultAsync(p => p.Id == request.PlaceId);
                if (place == null)
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse("Указанная площадка не существует"));
                }

                // Проверяем существование услуг
                foreach (var serviceId in request.ServiceIds)
                {
                    var service = await _context.Services.FindAsync(serviceId);
                    if (service == null)
                    {
                        return BadRequest(ApiResponse<object>.ErrorResponse($"Услуга с ID {serviceId} не существует"));
                    }
                }

                // Получаем ID текущего пользователя
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized(ApiResponse<object>.ErrorResponse("Пользователь не авторизован"));
                }

                // Проверяем существование пользователя
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return Unauthorized(ApiResponse<object>.ErrorResponse("Пользователь не найден"));
                }

                // Создаем новое событие
                var newEvent = new Event
                {
                    Name = request.EventName,
                    EventTypeId = request.EventTypeId
                };

                await _context.Events.AddAsync(newEvent);
                await _context.SaveChangesAsync();

                // Создаем заявку
                var application = new Application
                {
                    StatusId = 1, // "Новая"
                    EventId = newEvent.Id,
                    PlaceId = request.PlaceId,
                    UserId = userId,
                    DateCreate = DateTime.Now,
                    DateUpdate = DateTime.Now,
                    ContactPhone = request.ContactPhone,
                    GuestCount = request.GuestCount,
                    EventDate = request.EventDate,
                    EventTime = request.EventTime,
                    Duration = request.Duration,
                    AdditionalInfo = request.AdditionalInfo ?? string.Empty
                };

                await _context.Applications.AddAsync(application);
                await _context.SaveChangesAsync();

                // Добавляем услуги к заявке
                foreach (var serviceId in request.ServiceIds)
                {
                    await _context.ServicesApplications.AddAsync(new ServicesApplication
                    {
                        ApplicationId = application.Id,
                        ServiceId = serviceId
                    });
                }

                await _context.SaveChangesAsync();

                // Получаем созданную заявку с полными данными
                var createdApplication = await GetApplicationById(application.Id);
                return Ok(createdApplication);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse($"Ошибка при создании заявки: {ex.Message}"));
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateApplication(int id, [FromBody] UpdateApplicationRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    
                    return BadRequest(ApiResponse<object>.ErrorResponse("Ошибка валидации", errors));
                }

                var application = await _context.Applications
                    .Include(a => a.Event)
                    .Include(a => a.ServicesApplications)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (application == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResponse("Заявка не найдена"));
                }

                // Обновляем статус, если указан
                if (request.StatusId.HasValue)
                {
                    var status = await _context.Statuses.FindAsync(request.StatusId.Value);
                    if (status == null)
                    {
                        return BadRequest(ApiResponse<object>.ErrorResponse("Указанный статус не существует"));
                    }
                    application.StatusId = request.StatusId.Value;
                }

                // Обновляем событие, если указаны данные
                if (application.Event != null)
                {
                    if (!string.IsNullOrEmpty(request.EventName))
                    {
                        application.Event.Name = request.EventName;
                    }
                    
                    if (request.EventTypeId.HasValue)
                    {
                        var eventType = await _context.EventsTypes.FindAsync(request.EventTypeId.Value);
                        if (eventType == null)
                        {
                            return BadRequest(ApiResponse<object>.ErrorResponse("Указанный тип мероприятия не существует"));
                        }
                        application.Event.EventTypeId = request.EventTypeId.Value;
                    }
                }

                // Обновляем площадку
                if (request.PlaceId.HasValue)
                {
                    var place = await _context.Places.FindAsync(request.PlaceId.Value);
                    if (place == null)
                    {
                        return BadRequest(ApiResponse<object>.ErrorResponse("Указанная площадка не существует"));
                    }
                    application.PlaceId = request.PlaceId.Value;
                }

                // Обновляем услуги
                if (request.ServiceIds != null)
                {
                    // Удаляем старые услуги
                    _context.ServicesApplications.RemoveRange(application.ServicesApplications);
                    
                    // Добавляем новые услуги
                    foreach (var serviceId in request.ServiceIds)
                    {
                        var service = await _context.Services.FindAsync(serviceId);
                        if (service == null)
                        {
                            return BadRequest(ApiResponse<object>.ErrorResponse($"Услуга с ID {serviceId} не существует"));
                        }
                        
                        await _context.ServicesApplications.AddAsync(new ServicesApplication
                        {
                            ApplicationId = id,
                            ServiceId = serviceId
                        });
                    }
                }

                // Обновляем остальные поля
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

                // Получаем обновленную заявку с полными данными
                var updatedApplication = await GetApplicationById(id);
                return Ok(updatedApplication);
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse($"Ошибка при обновлении заявки: {ex.Message}"));
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteApplication(int id)
        {
            try
            {
                var application = await _context.Applications
                    .Include(a => a.ServicesApplications)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (application == null)
                {
                    return NotFound(ApiResponse<object>.ErrorResponse("Заявка не найдена"));
                }

                // Удаляем связанные услуги
                _context.ServicesApplications.RemoveRange(application.ServicesApplications);
                
                // Удаляем связанное событие
                var eventToDelete = await _context.Events.FindAsync(application.EventId);
                if (eventToDelete != null)
                {
                    _context.Events.Remove(eventToDelete);
                }

                // Удаляем заявку
                _context.Applications.Remove(application);
                await _context.SaveChangesAsync();

                return Ok(ApiResponse<object>.SuccessResponse(null, "Заявка успешно удалена"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse($"Ошибка при удалении заявки: {ex.Message}"));
            }
        }

        // Мои заявки
        [HttpGet("my")]
        public async Task<IActionResult> GetMyApplications()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized(ApiResponse<object>.ErrorResponse("Пользователь не авторизован"));
                }

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
                    EventTypeId = a.Event?.EventTypeId ?? 0,
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

                return Ok(ApiResponse<List<ApplicationResponse>>.SuccessResponse(response));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse($"Ошибка при получении заявок: {ex.Message}"));
            }
        }

        // НОВЫЙ МЕТОД: Мои заявки с полными данными
        [HttpGet("my/full")]
        public async Task<IActionResult> GetMyApplicationsWithFullDetails()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized(ApiResponse<object>.ErrorResponse("Пользователь не авторизован"));
                }

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

                var response = applications.Select(a => new
                {
                    a.Id,
                    a.StatusId,
                    Status = a.Status?.Name,
                    a.EventId,
                    Event = a.Event != null ? new
                    {
                        a.Event.Id,
                        a.Event.Name,
                        a.Event.EventTypeId,
                        EventType = a.Event.EventType?.NameEventsType
                    } : null,
                    a.PlaceId,
                    Place = a.Place != null ? new
                    {
                        a.Place.Id,
                        a.Place.Name,
                        Address = a.Place.Address != null ? new
                        {
                            a.Place.Address.Id,
                            a.Place.Address.City,
                            a.Place.Address.Street,
                            a.Place.Address.House,
                            FullAddress = $"{a.Place.Address.City}, {a.Place.Address.Street}, {a.Place.Address.House}"
                        } : null
                    } : null,
                    Services = a.ServicesApplications.Select(sa => new
                    {
                        sa.ServiceId,
                        ServiceName = sa.Service?.Name
                    }).ToList(),
                    ServiceIds = a.ServicesApplications.Select(sa => sa.ServiceId).ToList(),
                    a.DateCreate,
                    a.DateUpdate,
                    a.ContactPhone,
                    a.GuestCount,
                    a.EventDate,
                    a.EventTime,
                    a.Duration,
                    a.AdditionalInfo
                }).ToList();

                return Ok(ApiResponse<object>.SuccessResponse(response));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse($"Ошибка при получении заявок: {ex.Message}"));
            }
        }

        // Статистика заявок
        [HttpGet("stats")]
        public async Task<IActionResult> GetApplicationStats()
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

                var response = new
                {
                    TotalApplications = totalApplications,
                    ApplicationsToday = applicationsToday,
                    ApplicationsByStatus = applicationsByStatus,
                    ApplicationsByDay = applicationsByDay
                };

                return Ok(ApiResponse<object>.SuccessResponse(response));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse($"Ошибка при получении статистики: {ex.Message}"));
            }
        }
    }
}