using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhotographyCMS.Data;
using PhotographyCMS.Models;
using PhotographyCMS.DTOs;

namespace PhotographyCMS.Controllers
{
    [ApiController]
    [Route("api/blogs/{blogId}/comments")]
    public class CommentsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CommentsController(AppDbContext context)
        {
            _context = context;
        }

        // GET api/blogs/5/comments
        [HttpGet]
        public async Task<ActionResult<List<CommentReadDto>>> GetComments(int blogId)
        {
            var blogExists = await _context.Blogs.AnyAsync(b => b.Id == blogId);
            if (!blogExists) return NotFound("Blog not found.");

            var comments = await _context.Comments
                .Where(c => c.BlogId == blogId && c.IsApproved)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();

            var lookup = comments.ToDictionary(c => c.Id, c => new CommentReadDto
            {
                Id = c.Id,
                AuthorName = c.AuthorName,
                Content = c.Content,
                CreatedAt = c.CreatedAt,
                ParentCommentId = c.ParentCommentId,
                Replies = new List<CommentReadDto>()
            });

            var roots = new List<CommentReadDto>();

            foreach (var c in comments)
            {
                var dto = lookup[c.Id];
                if (c.ParentCommentId.HasValue && lookup.TryGetValue(c.ParentCommentId.Value, out var parentDto))
                    parentDto.Replies.Add(dto);
                else
                    roots.Add(dto);
            }

            return Ok(roots);
        }

        // POST api/blogs/5/comments
        [HttpPost]
        public async Task<ActionResult<CommentReadDto>> PostComment(int blogId, [FromBody] CommentCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var blogExists = await _context.Blogs.AnyAsync(b => b.Id == blogId);
            if (!blogExists) return NotFound("Blog not found.");

            if (dto.ParentCommentId.HasValue)
            {
                var parentExists = await _context.Comments
                    .AnyAsync(c => c.Id == dto.ParentCommentId.Value && c.BlogId == blogId);
                if (!parentExists) return BadRequest("Parent comment not found for this blog.");
            }

            var cleanName = WebUtility.HtmlEncode(dto.AuthorName.Trim());
            var cleanContent = WebUtility.HtmlEncode(dto.Content.Trim());

            var comment = new Comment
            {
                BlogId = blogId,
                ParentCommentId = dto.ParentCommentId,
                AuthorName = cleanName,
                // AuthorEmail = dto.AuthorEmail?.Trim(),
                Content = cleanContent,
                CreatedAt = DateTime.UtcNow,
                IsApproved = true
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            var result = new CommentReadDto
            {
                Id = comment.Id,
                AuthorName = comment.AuthorName,
                Content = comment.Content,
                CreatedAt = comment.CreatedAt,
                ParentCommentId = comment.ParentCommentId,
                Replies = new List<CommentReadDto>()
            };

            return CreatedAtAction(nameof(GetComments), new { blogId }, result);
        }

        // DELETE api/blogs/5/comments/12 (optional, for moderation)
        [HttpDelete("{commentId}")]
        public async Task<IActionResult> DeleteComment(int blogId, int commentId)
        {
            var comment = await _context.Comments
                .FirstOrDefaultAsync(c => c.Id == commentId && c.BlogId == blogId);

            if (comment == null) return NotFound();

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}