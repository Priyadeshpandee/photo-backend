using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhotographyCMS.Data;
using PhotographyCMS.Models;
using System.IO;

namespace PhotographyCMS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BlogsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BlogsController(AppDbContext context)
        {
            _context = context;
        }

        // GET ALL BLOGS
        [HttpGet]
        public async Task<IActionResult> GetBlogs()
        {
            var blogs = await _context.Blogs
                .Include(b => b.Images.OrderBy(i => i.Order))
                .ToListAsync();

            return Ok(blogs);
        }

        // CREATE BLOG (with multiple images)
        [HttpPost]
        public async Task<IActionResult> CreateBlog([FromForm] BlogCreateDto dto)
        {
            var blog = new Blog
            {
                Title = dto.Title,
                Category = dto.Category,
                Description = dto.Description
            };

            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            Directory.CreateDirectory(uploadsFolder);

            for (int i = 0; i < dto.Images.Count; i++)
            {
                var file = dto.Images[i];
                string fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                string filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                blog.Images.Add(new BlogImage
                {
                    ImageUrl = "/uploads/" + fileName,
                    Title = i < dto.ImageTitles.Count ? dto.ImageTitles[i] : "",
                    Order = i
                });
            }

            _context.Blogs.Add(blog);
            await _context.SaveChangesAsync();

            return Ok(blog);
        }

        // ADD a single image to an existing blog
        [HttpPost("{id}/images")]
        public async Task<IActionResult> AddImage(int id, [FromForm] AddImageDto dto)
        {
            var blog = await _context.Blogs.Include(b => b.Images).FirstOrDefaultAsync(b => b.Id == id);
            if (blog == null) return NotFound();

            string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            Directory.CreateDirectory(uploadsFolder);

            string fileName = Guid.NewGuid() + Path.GetExtension(dto.Image.FileName);
            string filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await dto.Image.CopyToAsync(stream);
            }

            var newImage = new BlogImage
            {
                ImageUrl = "/uploads/" + fileName,
                Title = dto.Title,
                Order = blog.Images.Count,
                BlogId = blog.Id
            };

            _context.BlogImages.Add(newImage);
            await _context.SaveChangesAsync();

            return Ok(newImage);
        }

        // DELETE a single image
        [HttpDelete("images/{imageId}")]
        public async Task<IActionResult> DeleteImage(int imageId)
        {
            var image = await _context.BlogImages.FindAsync(imageId);
            if (image == null) return NotFound();

            _context.BlogImages.Remove(image);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // DELETE BLOG
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBlog(int id)
        {
            var blog = await _context.Blogs.FindAsync(id);

            if (blog == null)
            {
                return NotFound();
            }

            _context.Blogs.Remove(blog);

            await _context.SaveChangesAsync();

            return Ok();
        }

        // UPDATE BLOG (title/category/description only — images managed via separate endpoints)
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBlog(int id, Blog updatedBlog)
        {
            var blog = await _context.Blogs.FindAsync(id);

            if (blog == null)
            {
                return NotFound();
            }

            blog.Title = updatedBlog.Title;
            blog.Category = updatedBlog.Category;
            blog.Description = updatedBlog.Description;

            await _context.SaveChangesAsync();

            return Ok(blog);
        }
    }
}