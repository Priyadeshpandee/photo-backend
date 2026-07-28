using System.Text.Json.Serialization;

namespace PhotographyCMS.Models
{
    public class GalleryCategory
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Slug { get; set; } = "";
        public string Description { get; set; } = "";

        [JsonIgnore]
        public ICollection<GalleryItem> GalleryItems { get; set; } = new List<GalleryItem>();
    }
}