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
            try
            {
                Console.WriteLine($"Getting all places, minRating: {minRating}");
                var response = await _placeService.GetAllPlacesAsync(minRating);
                
                Console.WriteLine($"GetAllPlaces success: {response.Success}, count: {response.Data?.Count}");
                
                if (!response.Success)
                {
                    Console.WriteLine($"Error in GetAllPlaces: {response.Message}");
                    return BadRequest(response);
                }
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EXCEPTION in GetAllPlaces: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, ApiResponse<List<PlaceResponse>>.ErrorResponse($"Внутренняя ошибка сервера: {ex.Message}"));
            }
        }

        [HttpGet("popular")]
        public async Task<IActionResult> GetPopularPlaces([FromQuery] int limit = 6)
        {
            try
            {
                Console.WriteLine($"Getting popular places, limit: {limit}");
                var response = await _placeService.GetPopularPlacesAsync(limit);
                
                Console.WriteLine($"GetPopularPlaces success: {response.Success}, count: {response.Data?.Count}");
                
                if (!response.Success)
                {
                    Console.WriteLine($"Error in GetPopularPlaces: {response.Message}");
                    return BadRequest(response);
                }
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EXCEPTION in GetPopularPlaces: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, ApiResponse<List<PlaceResponse>>.ErrorResponse($"Внутренняя ошибка сервера: {ex.Message}"));
            }
        }
        
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPlaceById(int id)
        {
            try
            {
                Console.WriteLine($"Getting place by ID: {id}");
                var response = await _placeService.GetPlaceByIdAsync(id);
                
                Console.WriteLine($"GetPlaceById success: {response.Success}");
                
                if (!response.Success)
                {
                    Console.WriteLine($"Error in GetPlaceById: {response.Message}");
                    return NotFound(response);
                }
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EXCEPTION in GetPlaceById: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, ApiResponse<PlaceResponse>.ErrorResponse($"Внутренняя ошибка сервера: {ex.Message}"));
            }
        }
        
        [HttpGet("search")]
        public async Task<IActionResult> SearchPlaces([FromQuery] string term)
        {
            try
            {
                Console.WriteLine($"Searching places with term: {term}");
                
                if (string.IsNullOrWhiteSpace(term))
                {
                    return BadRequest(ApiResponse<List<PlaceResponse>>.ErrorResponse("Поисковый запрос не может быть пустым"));
                }
                
                var response = await _placeService.SearchPlacesAsync(term);
                
                Console.WriteLine($"SearchPlaces success: {response.Success}, count: {response.Data?.Count}");
                
                if (!response.Success)
                {
                    Console.WriteLine($"Error in SearchPlaces: {response.Message}");
                    return BadRequest(response);
                }
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EXCEPTION in SearchPlaces: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, ApiResponse<List<PlaceResponse>>.ErrorResponse($"Внутренняя ошибка сервера: {ex.Message}"));
            }
        }
        
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreatePlace([FromBody] CreatePlaceRequest request)
        {
            try
            {
                Console.WriteLine($"Creating place: {request?.Name}");
                
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    
                    Console.WriteLine($"Model validation errors: {string.Join(", ", errors)}");
                    return BadRequest(ApiResponse<PlaceResponse>.ErrorResponse("Ошибка валидации", errors));
                }
                
                var response = await _placeService.CreatePlaceAsync(request);
                
                Console.WriteLine($"CreatePlace success: {response.Success}");
                
                if (!response.Success)
                {
                    Console.WriteLine($"Error in CreatePlace: {response.Message}");
                    return BadRequest(response);
                }
                
                return CreatedAtAction(nameof(GetPlaceById), new { id = response.Data.Id }, response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EXCEPTION in CreatePlace: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, ApiResponse<PlaceResponse>.ErrorResponse($"Внутренняя ошибка сервера: {ex.Message}"));
            }
        }
        
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdatePlace(int id, [FromBody] UpdatePlaceRequest request)
        {
            try
            {
                Console.WriteLine($"Updating place ID: {id}");
                
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    
                    Console.WriteLine($"Model validation errors: {string.Join(", ", errors)}");
                    return BadRequest(ApiResponse<PlaceResponse>.ErrorResponse("Ошибка валидации", errors));
                }
                
                var response = await _placeService.UpdatePlaceAsync(id, request);
                
                Console.WriteLine($"UpdatePlace success: {response.Success}");
                
                if (!response.Success)
                {
                    Console.WriteLine($"Error in UpdatePlace: {response.Message}");
                    return BadRequest(response);
                }
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EXCEPTION in UpdatePlace: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, ApiResponse<PlaceResponse>.ErrorResponse($"Внутренняя ошибка сервера: {ex.Message}"));
            }
        }
        
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeletePlace(int id)
        {
            try
            {
                Console.WriteLine($"Deleting place ID: {id}");
                var response = await _placeService.DeletePlaceAsync(id);
                
                Console.WriteLine($"DeletePlace success: {response.Success}");
                
                if (!response.Success)
                {
                    Console.WriteLine($"Error in DeletePlace: {response.Message}");
                    return BadRequest(response);
                }
                
                return Ok(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EXCEPTION in DeletePlace: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, ApiResponse<bool>.ErrorResponse($"Внутренняя ошибка сервера: {ex.Message}"));
            }
        }
    }
}
