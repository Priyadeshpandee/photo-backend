namespace PhotographyCMS.Models
{
    public class Blog
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public ICollection<BlogImage> Images { get; set; } = new List<BlogImage>();
    }
}