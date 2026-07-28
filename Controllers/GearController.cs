using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhotographyCMS.Data;
using PhotographyCMS.DTOs;
using PhotographyCMS.Models;

namespace PhotographyCMS.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GearController : ControllerBase
{
    private readonly AppDbContext _context;
    public GearController(AppDbContext context) => _context = context;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var gear = await _context.GearItems
            .OrderBy(g => g.SortOrder)
            .Select(g => new GearItemDto { Id = g.Id, Name = g.Name, SortOrder = g.SortOrder })
            .ToListAsync();
        return Ok(gear);
    }

    [HttpPost]
    public async Task<IActionResult> Create(GearItemDto dto)
    {
        var item = new GearItem { Name = dto.Name, SortOrder = dto.SortOrder };
        _context.GearItems.Add(item);
        await _context.SaveChangesAsync();
        return Ok(item);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, GearItemDto dto)
    {
        var item = await _context.GearItems.FindAsync(id);
        if (item == null) return NotFound();
        item.Name = dto.Name;
        item.SortOrder = dto.SortOrder;
        await _context.SaveChangesAsync();
        return Ok(item);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _context.GearItems.FindAsync(id);
        if (item == null) return NotFound();
        _context.GearItems.Remove(item);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}