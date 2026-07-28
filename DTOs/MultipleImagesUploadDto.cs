using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace PhotographyCMS.DTOs
{
    public class MultipleImagesUploadDto
    {
        public List<IFormFile>? Images { get; set; }
        public List<string>? Titles { get; set; }
    }
}
