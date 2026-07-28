using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhotographyCMS.Data;
using PhotographyCMS.DTOs;
using PhotographyCMS.Models;

namespace PhotographyCMS.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AwardsController : ControllerBase
{
    private readonly AppDbContext _context;

    public AwardsController(AppDbContext context)
    {
        _context = context;
    }

    // GET /api/awards
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var awards = await _context.Awards
            .OrderByDescending(a => a.Year)
            .ToListAsync();

        var result = awards.Select(a => new AwardDto
        {
            Id = a.Id,
            AwardName = a.AwardName,
            Country = a.Country,
            Year = a.Year,
            Competition = a.Competition,
            Title = a.Title
        });

        return Ok(result);
    }

    // GET /api/awards/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var a = await _context.Awards.FindAsync(id);
        if (a == null) return NotFound();

        return Ok(new AwardDto
        {
            Id = a.Id,
            AwardName = a.AwardName,
            Country = a.Country,
            Year = a.Year,
            Competition = a.Competition,
            Title = a.Title
        });
    }

   
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUpdateAwardDto dto)
    {
        var award = new Award
        {
            AwardName = dto.AwardName,
            Country = dto.Country,
            Year = dto.Year,
            Competition = dto.Competition,
            Title = dto.Title
        };

        _context.Awards.Add(award);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = award.Id }, award);
    }

   
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateUpdateAwardDto dto)
    {
        var award = await _context.Awards.FindAsync(id);
        if (award == null) return NotFound();

        award.AwardName = dto.AwardName;
        award.Country = dto.Country;
        award.Year = dto.Year;
        award.Competition = dto.Competition;
        award.Title = dto.Title;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var award = await _context.Awards.FindAsync(id);
        if (award == null) return NotFound();

        _context.Awards.Remove(award);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}