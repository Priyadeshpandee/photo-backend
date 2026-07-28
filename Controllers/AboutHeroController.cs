using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using PhotographyCMS.Data;
using PhotographyCMS.DTOs;
using PhotographyCMS.Models;

namespace PhotographyCMS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AboutHeroController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public AboutHeroController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: api/abouthero
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var hero = await _context.AboutHeroes.FirstOrDefaultAsync();

                if (hero == null)
                    return NotFound(new { message = "No hero content found" });

                // Convert relative image paths to full URLs
                if (!string.IsNullOrEmpty(hero.Image) && !hero.Image.StartsWith("http"))
                {
                    hero.Image = $"{Request.Scheme}://{Request.Host}/uploads/{hero.Image}";
                }

                return Ok(hero);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error retrieving hero content", error = ex.Message });
            }
        }

        // POST: api/abouthero
        [HttpPost]
        public async Task<IActionResult> Create()
        {
            try
            {
               
                if (!Request.Form.ContainsKey("name") || !Request.Form.ContainsKey("subtitle") || !Request.Form.ContainsKey("bio"))
                {
                    return BadRequest(new { message = "Missing required fields: name, subtitle, bio" });
                }

                string name = Request.Form["name"].ToString();
                string subtitle = Request.Form["subtitle"].ToString();
                string bio = Request.Form["bio"].ToString();
                IFormFile? image = Request.Form.Files.FirstOrDefault(f => f.Name == "image");

                string imagePath = string.Empty;

                // Check if hero already exists
                var existingHero = await _context.AboutHeroes.FirstOrDefaultAsync();
                if (existingHero != null)
                {
                    return BadRequest(new { message = "Hero content already exists. Use PUT to update." });
                }

                // Handle image upload
                if (image != null && image.Length > 0)
                {
                    imagePath = await SaveImageAsync(image);
                }

                var hero = new AboutHero
                {
                    Name = name,
                    Subtitle = subtitle,
                    Bio = bio,
                    Image = imagePath
                };

                _context.AboutHeroes.Add(hero);
                await _context.SaveChangesAsync();

                // Return with full URL
                if (!string.IsNullOrEmpty(hero.Image) && !hero.Image.StartsWith("http"))
                {
                    hero.Image = $"{Request.Scheme}://{Request.Host}/uploads/{hero.Image}";
                }

                return Ok(new { message = "Hero created successfully", data = hero });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error creating hero", error = ex.Message });
            }
        }

        // PUT: api/abouthero/1
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id)
        {
            try
            {
                var hero = await _context.AboutHeroes.FindAsync(id);

                if (hero == null)
                    return NotFound(new { message = "Hero not found" });

                // Parse form data
                if (Request.Form.ContainsKey("name"))
                    hero.Name = Request.Form["name"].ToString();

                if (Request.Form.ContainsKey("subtitle"))
                    hero.Subtitle = Request.Form["subtitle"].ToString();

                if (Request.Form.ContainsKey("bio"))
                    hero.Bio = Request.Form["bio"].ToString();

                // Handle image upload
                IFormFile? image = Request.Form.Files.FirstOrDefault(f => f.Name == "image");
                if (image != null && image.Length > 0)
                {
                    // Delete old image if exists
                    if (!string.IsNullOrEmpty(hero.Image))
                    {
                        await DeleteOldImageAsync(hero.Image);
                    }

                    hero.Image = await SaveImageAsync(image);
                }

                await _context.SaveChangesAsync();

                // Return with full URL
                if (!string.IsNullOrEmpty(hero.Image) && !hero.Image.StartsWith("http"))
                {
                    hero.Image = $"{Request.Scheme}://{Request.Host}/uploads/{hero.Image}";
                }

                return Ok(new { message = "Hero updated successfully", data = hero });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error updating hero", error = ex.Message });
            }
        }

        // DELETE: api/abouthero/1
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var hero = await _context.AboutHeroes.FindAsync(id);

                if (hero == null)
                    return NotFound(new { message = "Hero not found" });

                // Delete image file
                if (!string.IsNullOrEmpty(hero.Image))
                {
                    await DeleteOldImageAsync(hero.Image);
                }

                _context.AboutHeroes.Remove(hero);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Hero deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error deleting hero", error = ex.Message });
            }
        }

        // Helper method to save image
        private async Task<string> SaveImageAsync(IFormFile file)
        {
            try
            {
                // Validate file
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var fileExtension = Path.GetExtension(file.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    throw new Exception($"File type {fileExtension} is not allowed");
                }

                if (file.Length > 5 * 1024 * 1024) // 5MB limit
                {
                    throw new Exception("File size exceeds 5MB limit");
                }

                // Create uploads directory
                string uploadsDir = Path.Combine(_env.WebRootPath ?? "", "uploads");
                Directory.CreateDirectory(uploadsDir);

                // Generate unique filename
                string fileName = $"{Guid.NewGuid()}_{Path.GetFileNameWithoutExtension(file.FileName)}{fileExtension}";
                string filePath = Path.Combine(uploadsDir, fileName);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                return fileName;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error saving image: {ex.Message}");
            }
        }

        // Helper method to delete old image
        private async Task DeleteOldImageAsync(string imagePath)
        {
            try
            {
                if (string.IsNullOrEmpty(imagePath))
                    return;

                // Extract filename from URL if full URL
                if (imagePath.StartsWith("http"))
                {
                    imagePath = Path.GetFileName(imagePath);
                }

                string filePath = Path.Combine(_env.WebRootPath ?? "", "uploads", imagePath);
                
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                    await Task.CompletedTask;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Error deleting old image: {ex.Message}");
                // Don't throw - just log the warning
            }
        }
    }
}