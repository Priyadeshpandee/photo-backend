using Microsoft.AspNetCore.Http;

namespace PhotographyCMS.DTOs
{
    public class ImageUploadDto
    {
        public IFormFile? Image { get; set; }
    }
}
