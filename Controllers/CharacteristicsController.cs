using MetaPlApi.Data.Entities;
using MetaPlApi.Models.DTOs.Responses;
using Microsoft.AspNetCore.Authorization;
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
        var characteristics = await _context.Characteristics
            .OrderBy(c => c.Name)
            .ToListAsync();
            
        var response = characteristics.Select(c => new
        {
            c.Id,
            c.Name
        }).ToList();
        
        return Ok(ApiResponse<object>.SuccessResponse(response));
    }
}
}
