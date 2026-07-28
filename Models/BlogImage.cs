namespace PhotographyCMS.Models
{
    public class BlogImage
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public int Order { get; set; }

        public int BlogId { get; set; }
        public Blog? Blog { get; set; }
    }
}