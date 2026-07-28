namespace PhotographyCMS.DTOs
{
    public class GalleryItemUploadDto
    {
        public string? Title { get; set; }
        public int CategoryId { get; set; }
        public IFormFile? Image { get; set; }
    }
}