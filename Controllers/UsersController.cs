using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MetaPlApi.Services;
using MetaPlApi.Models.DTOs.Requests;
using MetaPlApi.Models.DTOs.Responses;

namespace MetaPlApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        
        public UsersController(IUserService userService)
        {
            _userService = userService;
        }
        
        [HttpGet]
        public async Task<IActionResult> GetAllUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var response = await _userService.GetAllUsersAsync(page, pageSize);
            
            if (!response.Success)
            {
                return BadRequest(response);
            }
            
            return Ok(response);
        }
        
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var response = await _userService.GetUserByIdAsync(id);
            
            if (!response.Success)
            {
                return NotFound(response);
            }
            
            return Ok(response);
        }
        
        [HttpGet("role/{roleId}")]
        public async Task<IActionResult> GetUsersByRole(int roleId)
        {
            var response = await _userService.GetUsersByRoleAsync(roleId);
            
            if (!response.Success)
            {
                return BadRequest(response);
            }
            
            return Ok(response);
        }
        
        [HttpGet("search")]
        public async Task<IActionResult> SearchUsers([FromQuery] string term)
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                return BadRequest(ApiResponse<List<UserResponse>>.ErrorResponse("Поисковый запрос не может быть пустым"));
            }
            
            var response = await _userService.SearchUsersAsync(term);
            
            if (!response.Success)
            {
                return BadRequest(response);
            }
            
            return Ok(response);
        }
        
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                return Unauthorized(ApiResponse<object>.ErrorResponse("Не авторизован"));
            var response = await _userService.GetUserByIdAsync(userId);
            if (!response.Success)
                return NotFound(response);
            return Ok(response);
        }
        
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateProfileRequest request)
        {
            if (request == null)
                return BadRequest(ApiResponse<UserResponse>.ErrorResponse("Тело запроса не должно быть пустым. Укажите roleId для смены роли."));
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                
                return BadRequest(ApiResponse<UserResponse>.ErrorResponse("Ошибка валидации", errors));
            }
            
            var response = await _userService.UpdateUserAsync(id, request);
            
            if (!response.Success)
            {
                return BadRequest(response);
            }
            
            return Ok(response);
        }

        /// <summary>Сменить роль пользователя. Тело: { "roleId": number } (1 и выше).</summary>
        [HttpPut("{id}/role")]
        public async Task<IActionResult> ChangeUserRole(int id, [FromBody] ChangeUserRoleRequest request)
        {
            if (request == null || request.RoleId < 1)
                return BadRequest(ApiResponse<UserResponse>.ErrorResponse("Укажите roleId от 1 и выше."));
            
            var updateRequest = new UpdateProfileRequest { RoleId = request.RoleId };
            var response = await _userService.UpdateUserAsync(id, updateRequest);
            
            if (!response.Success)
                return BadRequest(response);
            
            return Ok(response);
        }
        
        [HttpPut("me")]
        public async Task<IActionResult> UpdateCurrentUser([FromBody] UpdateProfileRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                
                return BadRequest(ApiResponse<UserResponse>.ErrorResponse("Ошибка валидации", errors));
            }
            
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
            var response = await _userService.UpdateUserAsync(userId, request);
            
            if (!response.Success)
            {
                return BadRequest(response);
            }
            
            return Ok(response);
        }
        
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var response = await _userService.DeleteUserAsync(id);
            
            if (!response.Success)
            {
                return BadRequest(response);
            }
            
            return Ok(response);
        }
    }
}