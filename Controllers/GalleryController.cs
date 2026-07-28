using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhotographyCMS.Data;
using PhotographyCMS.DTOs;
using PhotographyCMS.Models;

namespace PhotographyCMS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GalleryController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public GalleryController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET /api/gallery/categories
        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _context.GalleryCategories.ToListAsync();
            return Ok(categories);
        }

        // GET /api/gallery?slug=award-winning
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? slug)
        {
            var query = _context.GalleryItems
                .Include(g => g.Category)
                .AsQueryable();

            if (!string.IsNullOrEmpty(slug))
                query = query.Where(g => g.Category.Slug == slug);

            var items = await query.ToListAsync();
            return Ok(items);
        }

        // GET /api/gallery/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _context.GalleryItems
                .Include(g => g.Category)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (item == null) return NotFound();
            return Ok(item);
        }

        // POST /api/gallery/upload
        [HttpPost("upload")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Upload([FromForm] GalleryItemUploadDto dto)
        {
            if (dto.Image == null)
                return BadRequest("No image provided.");

            var category = await _context.GalleryCategories.FindAsync(dto.CategoryId);
            if (category == null)
                return BadRequest("Invalid category.");

            var ext = Path.GetExtension(dto.Image.FileName).ToLower();
            var allowed = new List<string> { ".jpg", ".jpeg", ".png", ".webp" };
            if (!allowed.Contains(ext))
                return BadRequest("Invalid image type.");

            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "gallery");
            Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await dto.Image.CopyToAsync(stream);

            var item = new GalleryItem
            {
                Title = dto.Title ?? Path.GetFileNameWithoutExtension(dto.Image.FileName),
                ImageUrl = $"/uploads/gallery/{fileName}",
                CategoryId = dto.CategoryId,
            };

            _context.GalleryItems.Add(item);
            await _context.SaveChangesAsync();
            return Ok(item);
        }

        // PUT /api/gallery/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] GalleryItemUpdateDto dto)
        {
            var item = await _context.GalleryItems.FindAsync(id);
            if (item == null) return NotFound();

            item.Title = dto.Title ?? item.Title;
            item.CategoryId = dto.CategoryId ?? item.CategoryId;

            await _context.SaveChangesAsync();
            return Ok(item);
        }

        // PUT /api/gallery/5/image
        [HttpPut("{id}/image")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateImage(int id, [FromForm] GalleryItemImageUpdateDto dto)
        {
            var item = await _context.GalleryItems.FindAsync(id);
            if (item == null) return NotFound();

            if (dto.Image == null)
                return BadRequest("No image provided.");

            var ext = Path.GetExtension(dto.Image.FileName).ToLower();
            var allowed = new List<string> { ".jpg", ".jpeg", ".png", ".webp" };
            if (!allowed.Contains(ext))
                return BadRequest("Invalid image type.");

            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "gallery");
            Directory.CreateDirectory(uploadsFolder);

            var newFileName = $"{Guid.NewGuid()}{ext}";
            var newFilePath = Path.Combine(uploadsFolder, newFileName);

            using (var stream = new FileStream(newFilePath, FileMode.Create))
            {
                await dto.Image.CopyToAsync(stream);
            }

            // Delete old file
            var oldFilePath = Path.Combine(_env.WebRootPath,
                item.ImageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (System.IO.File.Exists(oldFilePath))
                System.IO.File.Delete(oldFilePath);

            item.ImageUrl = $"/uploads/gallery/{newFileName}";
            await _context.SaveChangesAsync();
            return Ok(item);
        }

        // DELETE /api/gallery/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.GalleryItems.FindAsync(id);
            if (item == null) return NotFound();

            var filePath = Path.Combine(_env.WebRootPath,
                item.ImageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);

            _context.GalleryItems.Remove(item);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}