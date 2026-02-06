using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MetaPlApi.Services;
using MetaPlApi.Models.DTOs.Requests;
using MetaPlApi.Models.DTOs.Responses;

namespace MetaPlApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }
        
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                Console.WriteLine($"Login attempt for user: {request?.Login}");
                
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    
                    Console.WriteLine($"Model validation errors: {string.Join(", ", errors)}");
                    return BadRequest(ApiResponse<AuthResponse>.ErrorResponse("Ошибка валидации", errors));
                }
                
                var response = await _authService.LoginAsync(request);
                
                Console.WriteLine($"Login response success: {response.Success}, message: {response.Message}");
                
                if (!response.Success)
                {
                    return Unauthorized(response);
                }
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in Login: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, ApiResponse<AuthResponse>.ErrorResponse($"Внутренняя ошибка сервера: {ex.Message}"));
            }
        }
        
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                Console.WriteLine($"Register attempt for user: {request?.Login}");
                
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    
                    Console.WriteLine($"Model validation errors: {string.Join(", ", errors)}");
                    return BadRequest(ApiResponse<AuthResponse>.ErrorResponse("Ошибка валидации", errors));
                }
                
                var response = await _authService.RegisterAsync(request);
                
                Console.WriteLine($"Register response success: {response.Success}, message: {response.Message}");
                
                if (!response.Success)
                {
                    return BadRequest(response);
                }
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in Register: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, ApiResponse<AuthResponse>.ErrorResponse($"Внутренняя ошибка сервера: {ex.Message}"));
            }
        }
        
        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                Console.WriteLine("Change password attempt");
                
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    
                    Console.WriteLine($"Model validation errors: {string.Join(", ", errors)}");
                    return BadRequest(ApiResponse<bool>.ErrorResponse("Ошибка валидации", errors));
                }
                
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    Console.WriteLine("User not authorized for password change");
                    return Unauthorized(ApiResponse<bool>.ErrorResponse("Пользователь не авторизован"));
                }
                
                Console.WriteLine($"Changing password for user ID: {userId}");
                var response = await _authService.ChangePasswordAsync(userId, request);
                
                Console.WriteLine($"Change password response success: {response.Success}");
                
                if (!response.Success)
                {
                    return BadRequest(response);
                }
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR in ChangePassword: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, ApiResponse<bool>.ErrorResponse($"Внутренняя ошибка сервера: {ex.Message}"));
            }
        }
    }
}
