using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PhotographyCMS.Models
{
    public class Comment
    {
        public int Id { get; set; }

        [Required]
        public int BlogId { get; set; }

        [ForeignKey(nameof(BlogId))]
        public Blog? Blog { get; set; }

        // Null = top-level comment. Set = this is a reply to another comment.
        public int? ParentCommentId { get; set; }

        [ForeignKey(nameof(ParentCommentId))]
        public Comment? ParentComment { get; set; }

        public ICollection<Comment> Replies { get; set; } = new List<Comment>();

        [Required]
        [MaxLength(100)]
        public string AuthorName { get; set; } = string.Empty;

        // Stored but never returned in API responses - used for moderation only
        [MaxLength(200)]
        public string? AuthorEmail { get; set; }

        [Required]
        [MaxLength(2000)]
        public string Content { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Optional: lets you hide spam without deleting it
        public bool IsApproved { get; set; } = true;
    }
}