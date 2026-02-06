using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MetaPlApi.Data.Entities;
using MetaPlApi.Models.DTOs.Responses;
using System.Security.Claims;

namespace MetaPlApi.Controllers
{
    [Route("api/places/{placeId:int}/reviews")]
    [ApiController]
    public class PlaceReviewsController : ControllerBase
    {
        private readonly MetaplatformeContext _context;

        public PlaceReviewsController(MetaplatformeContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetReviews(int placeId, [FromQuery] int limit = 50)
        {
            var place = await _context.Places.FindAsync(placeId);
            if (place == null) return NotFound(ApiResponse<object>.ErrorResponse("Площадка не найдена"));

            var list = await _context.PlaceReviews
                .Where(r => r.PlaceId == placeId)
                .Include(r => r.User)
                .OrderByDescending(r => r.CreatedAt)
                .Take(limit)
                .Select(r => new
                {
                    r.Id,
                    r.PlaceId,
                    r.UserId,
                    UserLogin = r.User.Login,
                    r.Rating,
                    r.Comment,
                    r.CreatedAt
                })
                .ToListAsync();

            var avg = list.Count > 0 ? Math.Round(list.Average(x => x.Rating), 1) : (double?)null;
            return Ok(ApiResponse<object>.SuccessResponse(new { averageRating = avg, reviewCount = list.Count, reviews = list }));
        }

        [HttpPost]
        public async Task<IActionResult> AddReview(int placeId, [FromBody] AddReviewRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                return Unauthorized(ApiResponse<object>.ErrorResponse("Не авторизован"));

            if (request.Rating < 1 || request.Rating > 5)
                return BadRequest(ApiResponse<object>.ErrorResponse("Оценка должна быть от 1 до 5"));

            var place = await _context.Places.FindAsync(placeId);
            if (place == null) return NotFound(ApiResponse<object>.ErrorResponse("Площадка не найдена"));

            var existing = await _context.PlaceReviews.FirstOrDefaultAsync(r => r.PlaceId == placeId && r.UserId == userId);
            var createdAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);

            if (existing != null)
            {
                existing.Rating = request.Rating;
                existing.Comment = request.Comment?.Trim();
                existing.CreatedAt = createdAt;
            }
            else
            {
                _context.PlaceReviews.Add(new PlaceReview
                {
                    PlaceId = placeId,
                    UserId = userId,
                    Rating = request.Rating,
                    Comment = request.Comment?.Trim(),
                    CreatedAt = createdAt
                });
            }
            await _context.SaveChangesAsync();
            return Ok(ApiResponse<object>.SuccessResponse(new { message = "Отзыв сохранён" }));
        }
    }

    public class AddReviewRequest
    {
        public int Rating { get; set; }
        public string? Comment { get; set; }
    }
}
