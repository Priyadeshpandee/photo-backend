namespace PhotographyCMS.Models
{
    public class AwardPhoto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string CompetitionName { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public int Year { get; set; }
    }
}