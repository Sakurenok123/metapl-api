
using Microsoft.AspNetCore.Mvc;

namespace MetaPlApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatusController : ControllerBase
    {
        [HttpGet]
        public IActionResult GetStatus()
        {
            return Ok(new 
            {
                Status = "Online",
                Timestamp = DateTime.Now,
                Version = "1.0"
            });
        }
    }
}