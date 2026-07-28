using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace PhotographyCMS.DTOs
{
    public class AwardPhotoUploadDto
    {
        public List<IFormFile>? Images { get; set; }
        public List<string>? Titles { get; set; }
        public string? CompetitionName { get; set; }
        public string? Country { get; set; }
        public int? Year { get; set; }
    }
}
