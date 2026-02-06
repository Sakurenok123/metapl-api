using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MetaPlApi.Data.Entities;
using MetaPlApi.Models.DTOs.Responses;

namespace MetaPlApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly MetaplatformeContext _context;
        
        public EventsController(MetaplatformeContext context)
        {
            _context = context;
        }
        
        [HttpGet]
        public async Task<IActionResult> GetAllEvents()
        {
            var events = await _context.Events
                .Include(e => e.EventType)
                .OrderBy(e => e.Name)
                .ToListAsync();
                
            var response = events.Select(e => new
            {
                e.Id,
                e.Name,
                EventTypeId = e.EventTypeId,
                EventTypeName = e.EventType.NameEventsType
            }).ToList();
            
            return Ok(ApiResponse<object>.SuccessResponse(response));
        }
    }
}