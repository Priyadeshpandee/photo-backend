using System.ComponentModel.DataAnnotations;

namespace PhotographyCMS.DTOs
{
    public class CommentCreateDto
    {
        [Required]
        [MaxLength(100)]
        public string AuthorName { get; set; } = string.Empty;

        [Required]
        [MaxLength(2000)]
        public string Content { get; set; } = string.Empty;

        // Set this when the comment is a reply to another comment
        public int? ParentCommentId { get; set; }
    }

    public class CommentReadDto
    {
        public int Id { get; set; }
        public string AuthorName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int? ParentCommentId { get; set; }
        public List<CommentReadDto> Replies { get; set; } = new();
    }
}