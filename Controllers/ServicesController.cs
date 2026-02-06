using MetaPlApi.Data.Entities;
using MetaPlApi.Models.DTOs.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MetaPlApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServicesController : ControllerBase
    {
        private readonly MetaplatformeContext _context;
        
        public ServicesController(MetaplatformeContext context)
        {
            _context = context;
        }
        
        [HttpGet]
        public async Task<IActionResult> GetAllServices()
        {
            var services = await _context.Services
                .OrderBy(s => s.Name)
                .ToListAsync();
                
            var response = services.Select(s => new ServiceInfo
            {
                Id = s.Id,
                Name = s.Name ?? "Не указано"
            }).ToList();
            
            return Ok(ApiResponse<List<ServiceInfo>>.SuccessResponse(response));
        }
    }
}
