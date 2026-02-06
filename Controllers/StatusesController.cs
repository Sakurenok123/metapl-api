using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MetaPlApi.Data.Entities;
using MetaPlApi.Models.DTOs.Responses;

namespace MetaPlApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatusesController : ControllerBase
    {
        private readonly MetaplatformeContext _context;
        
        public StatusesController(MetaplatformeContext context)
        {
            _context = context;
        }
        
        [HttpGet]
        public async Task<IActionResult> GetAllStatuses()
        {
            var statuses = await _context.Statuses
                .OrderBy(s => s.Name)
                .ToListAsync();
                
            var response = statuses.Select(s => new
            {
                s.Id,
                s.Name
            }).ToList();
            
            return Ok(ApiResponse<object>.SuccessResponse(response));
        }
    }
}