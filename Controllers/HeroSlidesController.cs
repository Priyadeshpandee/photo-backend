using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhotographyCMS.Data;
using PhotographyCMS.DTOs;
using PhotographyCMS.Models;

namespace PhotographyCMS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HeroSlidesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
        private static readonly string[] AllowedVideoExtensions = { ".mp4", ".webm", ".mov", ".m4v" };

        public HeroSlidesController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var slides = await _context.HeroSlides
                .Where(s => s.IsActive)
                .OrderBy(s => s.DisplayOrder)
                .ToListAsync();
            return Ok(slides);
        }

        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(200_000_000)] // videos are much bigger than photos — raise as needed
        public async Task<IActionResult> Upload([FromForm] MultipleImagesUploadDto dto)
        {
            var images = dto.Images ?? new List<IFormFile>();
            var titles = dto.Titles ?? new List<string>();
            if (images.Count == 0)
                return BadRequest("No images provided.");

            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "slides");
            Directory.CreateDirectory(uploadsFolder);

            int maxOrder = await _context.HeroSlides.AnyAsync()
                ? await _context.HeroSlides.MaxAsync(s => s.DisplayOrder) : 0;

            var saved = new List<HeroSlide>();

            for (int i = 0; i < images.Count; i++)
            {
                var file = images[i];
                var ext = Path.GetExtension(file.FileName).ToLower();
                var isImage = AllowedImageExtensions.Contains(ext);
                var isVideo = AllowedVideoExtensions.Contains(ext);
                if (!isImage && !isVideo) continue;

                var fileName = $"{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await file.CopyToAsync(stream);

                var slide = new HeroSlide
                {
                    Title = titles.ElementAtOrDefault(i) ?? Path.GetFileNameWithoutExtension(file.FileName),
                    ImageUrl = $"/uploads/slides/{fileName}",
                    MediaType = isVideo ? "video" : "image",
                    DisplayOrder = ++maxOrder,
                    IsActive = true
                };

                _context.HeroSlides.Add(slide);
                saved.Add(slide);
            }

            await _context.SaveChangesAsync();
            return Ok(saved);
        }

        [HttpPost("upload-image")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(200_000_000)]
        public async Task<IActionResult> UploadImageOnly([FromForm] ImageUploadDto dto)
        {
            var image = dto.Image;
            if (image == null)
                return BadRequest("No image provided.");

            var ext = Path.GetExtension(image.FileName).ToLower();
            var isImage = AllowedImageExtensions.Contains(ext);
            var isVideo = AllowedVideoExtensions.Contains(ext);
            if (!isImage && !isVideo)
                return BadRequest("Invalid file type.");

            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "slides");
            Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await image.CopyToAsync(stream);

            return Ok(new
            {
                imageUrl = $"/uploads/slides/{fileName}",
                mediaType = isVideo ? "video" : "image"
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] HeroSlideUpdateDto dto)
        {
            var slide = await _context.HeroSlides.FindAsync(id);
            if (slide == null) return NotFound();

            slide.Title = dto.Title ?? slide.Title;
            slide.IsActive = dto.IsActive ?? slide.IsActive;
            slide.DisplayOrder = dto.DisplayOrder ?? slide.DisplayOrder;
            slide.ImageUrl = dto.ImageUrl ?? slide.ImageUrl;
            slide.MediaType = dto.MediaType ?? slide.MediaType;

            await _context.SaveChangesAsync();
            return Ok(slide);
        }

        [HttpPut("reorder")]
        public async Task<IActionResult> Reorder([FromBody] List<int> orderedIds)
        {
            for (int i = 0; i < orderedIds.Count; i++)
            {
                var slide = await _context.HeroSlides.FindAsync(orderedIds[i]);
                if (slide != null) slide.DisplayOrder = i + 1;
            }
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var slide = await _context.HeroSlides.FindAsync(id);
            if (slide == null) return NotFound();

            var filePath = Path.Combine(_env.WebRootPath,
                slide.ImageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);

            _context.HeroSlides.Remove(slide);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }

    // public class MultipleImagesUploadDto
    // {
    //     public List<IFormFile>? Images { get; set; }   // ✅
    //     public List<string>? Titles { get; set; }
    // }
}