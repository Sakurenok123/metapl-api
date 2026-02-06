using MetaPlApi.Data.Entities;
using MetaPlApi.Models.DTOs.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MetaPlApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CharacteristicsController : ControllerBase
    {
        private readonly MetaplatformeContext _context;
        
        public CharacteristicsController(MetaplatformeContext context)
        {
            _context = context;
        }
        
        [HttpGet]
        public async Task<IActionResult> GetAllCharacteristics()
        {
            try
            {
                Console.WriteLine("Getting all characteristics");
                
                var characteristics = await _context.Characteristics
                    .OrderBy(c => c.Name)
                    .ToListAsync();
                
                Console.WriteLine($"Found {characteristics.Count} characteristics");
                
                var response = characteristics.Select(c => new CharacteristicInfo
                {
                    Id = c.Id,
                    Name = c.Name ?? "Не указано"
                }).ToList();
                
                return Ok(ApiResponse<List<CharacteristicInfo>>.SuccessResponse(response));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in GetAllCharacteristics: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, ApiResponse<List<CharacteristicInfo>>.ErrorResponse($"Ошибка при получении характеристик: {ex.Message}"));
            }
        }
    }
}
