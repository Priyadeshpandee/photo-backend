using Microsoft.AspNetCore.Http;

namespace PhotographyCMS.DTOs
{
    public class AwardPhotoImageUpdateDto
    {
        public IFormFile? Image { get; set; }
    }
}
