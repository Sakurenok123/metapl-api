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
            var addresses = await _context.Addresses
                .OrderBy(a => a.City)
                .ThenBy(a => a.Street)
                .ToListAsync();
            
            var response = addresses.Select(a => new
            {
                a.Id,
                a.City,
                a.Street,
                a.House,
                FullAddress = $"{a.City}, {a.Street}, {a.House}"
            }).ToList();
            
            return Ok(ApiResponse<object>.SuccessResponse(response));
        }

        [HttpGet("cities")]
        public async Task<IActionResult> GetCities()
        {
            var cities = await _context.Addresses
                .Select(a => a.City)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
                
            return Ok(ApiResponse<object>.SuccessResponse(cities));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAddressById(int id)
        {
            var address = await _context.Addresses.FindAsync(id);
            
            if (address == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse("Адрес не найден"));
            }
            
            var response = new
            {
                address.Id,
                address.City,
                address.Street,
                address.House,
                FullAddress = $"{address.City}, {address.Street}, {address.House}"
            };
            
            return Ok(ApiResponse<object>.SuccessResponse(response));
        }
        
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateAddress([FromBody] CreateAddressRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                
                return BadRequest(ApiResponse<object>.ErrorResponse("Ошибка валидации", errors));
            }
            
            var address = new Address
            {
                City = request.City,
                Street = request.Street,
                House = request.House
            };
            
            await _context.Addresses.AddAsync(address);
            await _context.SaveChangesAsync();
            
            var response = new
            {
                address.Id,
                address.City,
                address.Street,
                address.House,
                FullAddress = $"{address.City}, {address.Street}, {address.House}"
            };
            
            return CreatedAtAction(nameof(GetAddressById), new { id = address.Id }, 
                ApiResponse<object>.SuccessResponse(response, "Адрес успешно создан"));
        }
        
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateAddress(int id, [FromBody] UpdateAddressRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                
                return BadRequest(ApiResponse<object>.ErrorResponse("Ошибка валидации", errors));
            }
            
            var address = await _context.Addresses.FindAsync(id);
            
            if (address == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse("Адрес не найден"));
            }
            
            if (!string.IsNullOrEmpty(request.City))
            {
                address.City = request.City;
            }
            
            if (!string.IsNullOrEmpty(request.Street))
            {
                address.Street = request.Street;
            }
            
            if (request.House != null)
            {
                address.House = request.House;
            }
            
            await _context.SaveChangesAsync();
            
            var response = new
            {
                address.Id,
                address.City,
                address.Street,
                address.House,
                FullAddress = $"{address.City}, {address.Street}, {address.House}"
            };
            
            return Ok(ApiResponse<object>.SuccessResponse(response, "Адрес успешно обновлен"));
        }
        
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteAddress(int id)
        {
            var address = await _context.Addresses
                .Include(a => a.Places)
                .FirstOrDefaultAsync(a => a.Id == id);
            
            if (address == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse("Адрес не найден"));
            }
            
            // Проверяем, есть ли связанные места
            if (address.Places.Any())
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Невозможно удалить адрес, так как к нему привязаны места"));
            }
            
            _context.Addresses.Remove(address);
            await _context.SaveChangesAsync();
            
            return Ok(ApiResponse<object>.SuccessResponse(null, "Адрес успешно удален"));
        }
        
        [HttpGet("search")]
        public async Task<IActionResult> SearchAddresses([FromQuery] string city = null, [FromQuery] string street = null)
        {
            var query = _context.Addresses.AsQueryable();
            
            if (!string.IsNullOrEmpty(city))
            {
                query = query.Where(a => a.City.Contains(city));
            }
            
            if (!string.IsNullOrEmpty(street))
            {
                query = query.Where(a => a.Street.Contains(street));
            }
            
            var addresses = await query
                .OrderBy(a => a.City)
                .ThenBy(a => a.Street)
                .Take(50)
                .ToListAsync();
            
            var response = addresses.Select(a => new
            {
                a.Id,
                a.City,
                a.Street,
                a.House,
                FullAddress = $"{a.City}, {a.Street}, {a.House}"
            }).ToList();
            
            return Ok(ApiResponse<object>.SuccessResponse(response));
        }
    }
}
