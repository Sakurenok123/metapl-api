using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MetaPlApi.Services;
using MetaPlApi.Models.DTOs.Requests;
using MetaPlApi.Models.DTOs.Responses;

namespace MetaPlApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlacesController : ControllerBase
    {
        private readonly IPlaceService _placeService;
        
        public PlacesController(IPlaceService placeService)
        {
            _placeService = placeService;
        }
        
        [HttpGet]
        public async Task<IActionResult> GetAllPlaces([FromQuery] double? minRating = null)
        {
            var response = await _placeService.GetAllPlacesAsync(minRating);
            return Ok(response);
        }

        [HttpGet("popular")]
        public async Task<IActionResult> GetPopularPlaces([FromQuery] int limit = 6)
        {
            var response = await _placeService.GetPopularPlacesAsync(limit);
            return Ok(response);
        }
        
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPlaceById(int id)
        {
            var response = await _placeService.GetPlaceByIdAsync(id);
            
            if (!response.Success)
                return NotFound(response);
            
            return Ok(response);
        }
        
        [HttpGet("search")]
        public async Task<IActionResult> SearchPlaces([FromQuery] string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return BadRequest(ApiResponse<List<PlaceResponse>>.ErrorResponse("Поисковый запрос не может быть пустым"));
            
            var response = await _placeService.SearchPlacesAsync(term);
            return Ok(response);
        }
        
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreatePlace([FromBody] CreatePlaceRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                
                return BadRequest(ApiResponse<PlaceResponse>.ErrorResponse("Ошибка валидации", errors));
            }
            
            var response = await _placeService.CreatePlaceAsync(request);
            
            if (!response.Success)
                return BadRequest(response);
            
            return CreatedAtAction(nameof(GetPlaceById), new { id = response.Data?.Id }, response);
        }
        
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdatePlace(int id, [FromBody] UpdatePlaceRequest request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                
                return BadRequest(ApiResponse<PlaceResponse>.ErrorResponse("Ошибка валидации", errors));
            }
            
            var response = await _placeService.UpdatePlaceAsync(id, request);
            
            if (!response.Success)
                return BadRequest(response);
            
            return Ok(response);
        }
        
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeletePlace(int id)
        {
            var response = await _placeService.DeletePlaceAsync(id);
            
            if (!response.Success)
                return BadRequest(response);
            
            return Ok(response);
        }
    }
}
