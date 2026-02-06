using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MetaPlApi.Data.Entities;
using MetaPlApi.Models.DTOs.Requests;
using MetaPlApi.Models.DTOs.Responses;

namespace MetaPlApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AddressesController : ControllerBase
    {
        private readonly MetaplatformeContext _context;
        
        public AddressesController(MetaplatformeContext context)
        {
            _context = context;
        }
        
        [HttpGet]
        public async Task<IActionResult> GetAllAddresses()
        {
            try
            {
                var testData = new List<object>
                {
                    new { Id = 1, City = "Москва", Street = "Тверская", House = "1", FullAddress = "Москва, Тверская, 1" },
                    new { Id = 2, City = "Санкт-Петербург", Street = "Невский проспект", House = "2", FullAddress = "Санкт-Петербург, Невский проспект, 2" }
                };
                
                return Ok(ApiResponse<object>.SuccessResponse(testData));
            }
            catch (Exception ex)
            {
                return Ok(ApiResponse<object>.ErrorResponse($"Ошибка: {ex.Message}"));
            }
        }

        [HttpGet("cities")]
        public async Task<IActionResult> GetCities()
        {
            var cities = new List<string> { "Москва", "Санкт-Петербург", "Казань", "Екатеринбург" };
            return Ok(ApiResponse<object>.SuccessResponse(cities));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAddressById(int id)
        {
            var address = new 
            {
                Id = id,
                City = "Тестовый город",
                Street = "Тестовая улица",
                House = id.ToString(),
                FullAddress = $"Тестовый город, Тестовая улица, {id}"
            };
            
            return Ok(ApiResponse<object>.SuccessResponse(address));
        }
        
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateAddress([FromBody] CreateAddressRequest request)
        {
            var response = new
            {
                Id = 999,
                request.City,
                request.Street,
                request.House,
                FullAddress = $"{request.City}, {request.Street}, {request.House}"
            };
            
            return CreatedAtAction(nameof(GetAddressById), new { id = 999 }, 
                ApiResponse<object>.SuccessResponse(response, "Адрес успешно создан"));
        }
        
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateAddress(int id, [FromBody] UpdateAddressRequest request)
        {
            var response = new
            {
                Id = id,
                City = request.City ?? "Город",
                Street = request.Street ?? "Улица",
                House = request.House ?? "Дом",
                FullAddress = $"{request.City ?? "Город"}, {request.Street ?? "Улица"}, {request.House ?? "Дом"}"
            };
            
            return Ok(ApiResponse<object>.SuccessResponse(response, "Адрес успешно обновлен"));
        }
        
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteAddress(int id)
        {
            return Ok(ApiResponse<object>.SuccessResponse(null, "Адрес успешно удален"));
        }
        
        [HttpGet("search")]
        public async Task<IActionResult> SearchAddresses([FromQuery] string city = null, [FromQuery] string street = null)
        {
            var addresses = new List<object>
            {
                new { Id = 1, City = city ?? "Город", Street = street ?? "Улица", House = "1", FullAddress = $"{city ?? "Город"}, {street ?? "Улица"}, 1" }
            };
            
            return Ok(ApiResponse<object>.SuccessResponse(addresses));
        }
    }
}
