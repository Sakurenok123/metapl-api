using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MetaPlApi.Data.Entities;
using MetaPlApi.Models.DTOs.Responses;

namespace MetaPlApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PhotosController : ControllerBase
    {
        private readonly MetaplatformeContext _context;
        private readonly IWebHostEnvironment _env;

        public PhotosController(MetaplatformeContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        /// <summary>
        /// Создать запись фото по URL (ссылке в интернете).
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> CreateByUrl([FromBody] CreatePhotoRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Name))
                return BadRequest(ApiResponse<object>.ErrorResponse("Укажите URL или путь к фото"));

            var photo = new Photo { Name = request.Name.Trim() };
            _context.Photos.Add(photo);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<object>.SuccessResponse(new { id = photo.Id, name = photo.Name }));
        }

        /// <summary>
        /// Загрузить файл с компьютера и создать запись фото.
        /// </summary>
        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(ApiResponse<object>.ErrorResponse("Файл не выбран или пуст"));

            var allowed = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
            if (!allowed.Contains(file.ContentType.ToLowerInvariant()))
                return BadRequest(ApiResponse<object>.ErrorResponse("Допустимы только изображения: JPEG, PNG, GIF, WebP"));

            var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
            var uploadsDir = Path.Combine(webRoot, "uploads");
            if (!Directory.Exists(uploadsDir))
                Directory.CreateDirectory(uploadsDir);

            var ext = Path.GetExtension(file.FileName);
            if (string.IsNullOrEmpty(ext)) ext = ".jpg";
            var fileName = $"{Guid.NewGuid():N}{ext}";
            var fullPath = Path.Combine(uploadsDir, fileName);

            await using (var stream = new FileStream(fullPath, FileMode.Create))
                await file.CopyToAsync(stream);

            var relativePath = "/uploads/" + fileName;
            var photo = new Photo { Name = relativePath };
            _context.Photos.Add(photo);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<object>.SuccessResponse(new { id = photo.Id, name = photo.Name }));
        }
    }

    public class CreatePhotoRequest
    {
        public string Name { get; set; } = string.Empty;
    }
}
