using MetaPlApi.Data.Entities;
using MetaPlApi.Models.DTOs.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MetaPlApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EquipmentsController : ControllerBase
    {
        private readonly MetaplatformeContext _context;
        
        public EquipmentsController(MetaplatformeContext context)
        {
            _context = context;
        }
    
        [HttpGet]
        public async Task<IActionResult> GetAllEquipments()
        {
            try
            {
                var equipment = await _context.Equipments
                    .OrderBy(e => e.Name)
                    .ToListAsync();
                    
                var response = equipment.Select(e => new EquipmentInfo
                {
                    Id = e.Id,
                    Name = e.Name
                }).ToList();
                
                return Ok(new ApiResponse<List<EquipmentInfo>>
                {
                    Success = true,
                    Message = "Успешно",
                    Data = response
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new ApiResponse<List<EquipmentInfo>>
                {
                    Success = false,
                    Message = $"Ошибка при получении оборудования: {ex.Message}",
                    Data = null
                });
            }
        }
    }
}