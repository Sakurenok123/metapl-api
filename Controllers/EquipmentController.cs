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
            var equipment = await _context.Equipments
                .OrderBy(e => e.Name)
                .ToListAsync();
                
            var response = equipment.Select(e => new EquipmentInfo
            {
                Id = e.Id,
                Name = e.Name ?? "Не указано"
            }).ToList();
            
            return Ok(ApiResponse<List<EquipmentInfo>>.SuccessResponse(response));
        }
    }
}
