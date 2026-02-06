using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MetaPlApi.Models.DTOs.Responses;
using System.Security.Claims;

namespace MetaPlApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        [HttpGet("me")]
        [Authorize]
        public IActionResult GetCurrentUser()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var login = User.FindFirst(ClaimTypes.Name)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;
            
            var response = new UserResponse
            {
                Id = userId != null ? int.Parse(userId) : 0,
                Login = login ?? "Неизвестно",
                Role = role ?? "Пользователь"
            };
            
            return Ok(ApiResponse<UserResponse>.SuccessResponse(response));
        }
        
        // Простой endpoint без авторизации для тестирования
        [HttpGet("test")]
        public IActionResult Test()
        {
            var response = new UserResponse
            {
                Id = 1,
                Login = "testuser",
                Role = "Пользователь"
            };
            
            return Ok(ApiResponse<UserResponse>.SuccessResponse(response));
        }
    }
}
