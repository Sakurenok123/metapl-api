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
            try
            {
                Console.WriteLine("Getting all services");
                
                var services = await _context.Services
                    .OrderBy(s => s.Name)
                    .ToListAsync();
                
                Console.WriteLine($"Found {services.Count} services");
                
                var response = services.Select(s => new ServiceInfo
                {
                    Id = s.Id,
                    Name = s.Name ?? "Не указано"
                }).ToList();
                
                return Ok(ApiResponse<List<ServiceInfo>>.SuccessResponse(response));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in GetAllServices: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, ApiResponse<List<ServiceInfo>>.ErrorResponse($"Ошибка при получении услуг: {ex.Message}"));
            }
        }
    }
}
