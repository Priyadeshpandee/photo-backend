using Microsoft.AspNetCore.Mvc;

namespace PhotographyCMS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UploadController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded");
            }

            var uploadsFolder = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "uploads"
            );

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var fileName =
                Guid.NewGuid().ToString() +
                Path.GetExtension(file.FileName);

            var filePath = Path.Combine(
                uploadsFolder,
                fileName
            );

            using (var stream = new FileStream(
                filePath,
                FileMode.Create
            ))
            {
                await file.CopyToAsync(stream);
            }

            var imageUrl =
                $"{Request.Scheme}://{Request.Host}/uploads/{fileName}";

            return Ok(new
            {
                imageUrl
            });
        }
    }
}