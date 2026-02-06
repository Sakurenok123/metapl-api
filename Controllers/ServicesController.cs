using MetaPlApi.Data.Entities;
using MetaPlApi.Models.DTOs.Responses;
using Microsoft.AspNetCore.Authorization;
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
            
        var response = services.Select(s => new
        {
            s.Id,
            s.Name
        }).ToList();
        
        return Ok(ApiResponse<object>.SuccessResponse(response));
    }
}
}
