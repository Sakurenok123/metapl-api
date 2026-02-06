using Microsoft.AspNetCore.Mvc;
using MetaPlApi.Data.Entities;
using MetaPlApi.Models.DTOs.Responses;
using Microsoft.EntityFrameworkCore;

namespace MetaPlApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsTypeController : ControllerBase
    {
        private readonly MetaplatformeContext _context;
        public EventsTypeController(MetaplatformeContext context) => _context = context;

        [HttpGet]
        public async Task<IActionResult> GetAllEventTypes()
        {
            try
            {
                var types = await _context.EventsTypes.ToListAsync();
                var response = types.Select(t => new { 
                    Id = t.Id, 
                    Name = t.NameEventsType 
                }).ToList();
                return Ok(new { Success = true, Data = response });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Success = false, Message = ex.Message });
            }
        }
    }
}