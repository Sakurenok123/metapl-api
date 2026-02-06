using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MetaPlApi.Data.Entities;
using System.Security.Claims;

namespace MetaPlApi.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UserProfileController : ControllerBase
    {
        private readonly MetaplatformeContext _context;

        public UserProfileController(MetaplatformeContext context)
        {
            _context = context;
        }

        private int? GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(claim) || !int.TryParse(claim, out int userId))
                return null;
            return userId;
        }

        // ========== ИЗБРАННОЕ ==========

        [HttpGet("favorites")]
        public async Task<IActionResult> GetFavorites()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var list = await _context.UserFavorites
                .Where(uf => uf.UserId == userId)
                .Include(uf => uf.Place)
                .ThenInclude(p => p.Address)
                .OrderByDescending(uf => uf.AddedDate)
                .Select(uf => new
                {
                    uf.Id,
                    uf.PlaceId,
                    uf.AddedDate,
                    Place = new
                    {
                        uf.Place.Id,
                        uf.Place.Name,
                        Address = uf.Place.Address == null ? null : new
                        {
                            uf.Place.Address.City,
                            uf.Place.Address.Street,
                            uf.Place.Address.House,
                            FullAddress = uf.Place.Address.City + ", " + uf.Place.Address.Street + (string.IsNullOrEmpty(uf.Place.Address.House) ? "" : ", " + uf.Place.Address.House)
                        }
                    }
                })
                .ToListAsync();

            return Ok(new { Success = true, Data = list });
        }

        [HttpPost("favorites/{placeId:int}")]
        public async Task<IActionResult> AddFavorite(int placeId)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var place = await _context.Places.FindAsync(placeId);
            if (place == null) return NotFound(new { Success = false, Message = "Площадка не найдена" });

            var exists = await _context.UserFavorites
                .AnyAsync(uf => uf.UserId == userId && uf.PlaceId == placeId);
            if (exists) return Ok(new { Success = true, Message = "Уже в избранном" });

            _context.UserFavorites.Add(new UserFavorite
            {
                UserId = userId.Value,
                PlaceId = placeId,
                AddedDate = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified)
            });
            await _context.SaveChangesAsync();
            return Ok(new { Success = true, Message = "Добавлено в избранное" });
        }

        [HttpDelete("favorites/{placeId:int}")]
        public async Task<IActionResult> RemoveFavorite(int placeId)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var item = await _context.UserFavorites
                .FirstOrDefaultAsync(uf => uf.UserId == userId && uf.PlaceId == placeId);
            if (item != null)
            {
                _context.UserFavorites.Remove(item);
                await _context.SaveChangesAsync();
            }
            return Ok(new { Success = true, Message = "Удалено из избранного" });
        }

        [HttpGet("favorites/{placeId:int}/check")]
        public async Task<IActionResult> CheckFavorite(int placeId)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var isFavorite = await _context.UserFavorites
                .AnyAsync(uf => uf.UserId == userId && uf.PlaceId == placeId);
            return Ok(new { Success = true, Data = isFavorite });
        }

        // ========== ИСТОРИЯ ПРОСМОТРОВ ==========

        [HttpGet("view-history")]
        public async Task<IActionResult> GetViewHistory([FromQuery] int limit = 20)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var list = await _context.UserViewHistories
                .Where(h => h.UserId == userId)
                .Include(h => h.Place)
                .ThenInclude(p => p.Address)
                .OrderByDescending(h => h.ViewedAt)
                .Take(limit)
                .Select(h => new
                {
                    h.Id,
                    h.PlaceId,
                    h.ViewedAt,
                    Place = new
                    {
                        h.Place.Id,
                        h.Place.Name,
                        Address = h.Place.Address == null ? null : new
                        {
                            h.Place.Address.City,
                            h.Place.Address.Street,
                            h.Place.Address.House,
                            FullAddress = h.Place.Address.City + ", " + h.Place.Address.Street + (string.IsNullOrEmpty(h.Place.Address.House) ? "" : ", " + h.Place.Address.House)
                        }
                    }
                })
                .ToListAsync();

            return Ok(new { Success = true, Data = list });
        }

        [HttpPost("view-history/{placeId:int}")]
        public async Task<IActionResult> AddToViewHistory(int placeId)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var place = await _context.Places.FindAsync(placeId);
            if (place == null) return NotFound(new { Success = false, Message = "Площадка не найдена" });

            var existing = await _context.UserViewHistories
                .FirstOrDefaultAsync(h => h.UserId == userId && h.PlaceId == placeId);
            var dateUnspecified = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            if (existing != null)
            {
                existing.ViewedAt = dateUnspecified;
                await _context.SaveChangesAsync();
                return Ok(new { Success = true });
            }

            _context.UserViewHistories.Add(new UserViewHistory
            {
                UserId = userId.Value,
                PlaceId = placeId,
                ViewedAt = dateUnspecified
            });
            await _context.SaveChangesAsync();
            return Ok(new { Success = true });
        }

        [HttpDelete("view-history")]
        public async Task<IActionResult> ClearViewHistory()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var items = await _context.UserViewHistories.Where(h => h.UserId == userId).ToListAsync();
            _context.UserViewHistories.RemoveRange(items);
            await _context.SaveChangesAsync();
            return Ok(new { Success = true, Message = "История очищена" });
        }
    }
}
