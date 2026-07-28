using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhotographyCMS.Data;
using PhotographyCMS.DTOs;
using PhotographyCMS.Models;

namespace PhotographyCMS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AboutController : ControllerBase
    {
        private readonly AppDbContext _context;
        public AboutController(AppDbContext context) => _context = context;

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var stats = await _context.AboutStats
                .OrderBy(s => s.SortOrder)
                .Select(s => new AboutStatDto { Id = s.Id, Label = s.Label, Value = s.Value, SortOrder = s.SortOrder })
                .ToListAsync();
            return Ok(stats);
        }

        [HttpPost("stats")]
        public async Task<IActionResult> CreateStat(AboutStatDto dto)
        {
            var stat = new AboutStat { Label = dto.Label, Value = dto.Value, SortOrder = dto.SortOrder };
            _context.AboutStats.Add(stat);
            await _context.SaveChangesAsync();
            return Ok(stat);
        }

        [HttpPut("stats/{id}")]
        public async Task<IActionResult> UpdateStat(int id, AboutStatDto dto)
        {
            var stat = await _context.AboutStats.FindAsync(id);
            if (stat == null) return NotFound();
            stat.Label = dto.Label;
            stat.Value = dto.Value;
            stat.SortOrder = dto.SortOrder;
            await _context.SaveChangesAsync();
            return Ok(stat);
        }

        [HttpDelete("stats/{id}")]
        public async Task<IActionResult> DeleteStat(int id)
        {
            var stat = await _context.AboutStats.FindAsync(id);
            if (stat == null) return NotFound();
            _context.AboutStats.Remove(stat);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}