namespace PhotographyCMS.Models
{
    public class BlogCreateDto
    {
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public List<IFormFile> Images { get; set; } = new();
        public List<string> ImageTitles { get; set; } = new();
    }

    public class AddImageDto
    {
        public IFormFile Image { get; set; } = null!;
        public string Title { get; set; } = string.Empty;
    }
}