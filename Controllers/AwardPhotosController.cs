using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhotographyCMS.Data;
using PhotographyCMS.DTOs;
using PhotographyCMS.Models;

namespace PhotographyCMS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AwardPhotosController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public AwardPhotosController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var photos = await _context.AwardPhotos.ToListAsync();
            return Ok(photos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var photo = await _context.AwardPhotos.FindAsync(id);
            if (photo == null) return NotFound();
            return Ok(photo);
        }

        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Upload([FromForm] AwardPhotoUploadDto dto)
        {
            var images = dto.Images ?? new List<IFormFile>();
            var titles = dto.Titles ?? new List<string>();

            if (images.Count == 0)
                return BadRequest("No images provided.");

            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "awards");
            Directory.CreateDirectory(uploadsFolder);

            var saved = new List<AwardPhoto>();

            for (int i = 0; i < images.Count; i++)
            {
                var file = images[i];
                var ext = Path.GetExtension(file.FileName).ToLower();
                var allowed = new List<string> { ".jpg", ".jpeg", ".png", ".webp" };
                if (!allowed.Contains(ext)) continue;

                var fileName = $"{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await file.CopyToAsync(stream);

                var photo = new AwardPhoto
                {
                    Title = titles.ElementAtOrDefault(i) ?? Path.GetFileNameWithoutExtension(file.FileName),
                    ImageUrl = $"/uploads/awards/{fileName}",
                    CompetitionName = dto.CompetitionName ?? string.Empty,
                    Country = dto.Country ?? string.Empty,
                    Year = dto.Year ?? DateTime.UtcNow.Year
                };

                _context.AwardPhotos.Add(photo);
                saved.Add(photo);
            }

            await _context.SaveChangesAsync();
            return Ok(saved);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] AwardPhotoUpdateDto dto)
        {
            var photo = await _context.AwardPhotos.FindAsync(id);
            if (photo == null) return NotFound();

            photo.Title = dto.Title ?? photo.Title;
            photo.CompetitionName = dto.CompetitionName ?? photo.CompetitionName;
            photo.Country = dto.Country ?? photo.Country;
            photo.Year = dto.Year ?? photo.Year;
            photo.ImageUrl = dto.ImageUrl ?? photo.ImageUrl;

            await _context.SaveChangesAsync();
            return Ok(photo);
        }

        [HttpPut("{id}/image")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateImage(int id, [FromForm] AwardPhotoImageUpdateDto dto)
        {
            var photo = await _context.AwardPhotos.FindAsync(id);
            if (photo == null) return NotFound();

            if (dto.Image == null)
                return BadRequest("No image provided.");

            var ext = Path.GetExtension(dto.Image.FileName).ToLower();
            var allowed = new List<string> { ".jpg", ".jpeg", ".png", ".webp" };
            if (!allowed.Contains(ext))
                return BadRequest("Invalid image type.");

            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "awards");
            Directory.CreateDirectory(uploadsFolder);

            var newFileName = $"{Guid.NewGuid()}{ext}";
            var newFilePath = Path.Combine(uploadsFolder, newFileName);

            using (var stream = new FileStream(newFilePath, FileMode.Create))
            {
                await dto.Image.CopyToAsync(stream);
            }

            var oldFilePath = Path.Combine(_env.WebRootPath,
                photo.ImageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (System.IO.File.Exists(oldFilePath))
                System.IO.File.Delete(oldFilePath);

            photo.ImageUrl = $"/uploads/awards/{newFileName}";
            await _context.SaveChangesAsync();
            return Ok(photo);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var photo = await _context.AwardPhotos.FindAsync(id);
            if (photo == null) return NotFound();

            var filePath = Path.Combine(_env.WebRootPath,
                photo.ImageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);

            _context.AwardPhotos.Remove(photo);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}