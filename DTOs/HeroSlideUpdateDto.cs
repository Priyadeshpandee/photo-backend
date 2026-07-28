namespace PhotographyCMS.DTOs
{
    public class HeroSlideUpdateDto
    {
        public string? Title { get; set; }
        public bool? IsActive { get; set; }
        public int? DisplayOrder { get; set; }
        public string? ImageUrl { get; set; }
    }
}