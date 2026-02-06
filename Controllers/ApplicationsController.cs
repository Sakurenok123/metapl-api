using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MetaPlApi.Models.DTOs.Responses;

namespace MetaPlApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApplicationsController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetAllApplications()
        {
            var response = ApiResponse<List<object>>.SuccessResponse(new List<object>
            {
                new { Id = 1, Status = "Новая", Date = DateTime.Now.AddDays(-1) },
                new { Id = 2, Status = "В работе", Date = DateTime.Now.AddDays(-2) },
                new { Id = 3, Status = "Завершена", Date = DateTime.Now.AddDays(-3) }
            });
            
            return Ok(response);
        }
    }
}
