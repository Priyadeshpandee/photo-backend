using System.Text.Json.Serialization;

namespace PhotographyCMS.Models
{
    public class GalleryItem
    {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string ImageUrl { get; set; } = "";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int CategoryId { get; set; }

        [JsonIgnore] 
        public GalleryCategory Category { get; set; } = null!;
    }
}