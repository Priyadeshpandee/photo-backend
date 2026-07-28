using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PhotographyCMS.Data;
using PhotographyCMS.Models;
using PhotographyCMS.DTOs;

[ApiController]
[Route("api/[controller]")]
public class ContactController : ControllerBase
{
    private readonly AppDbContext _context;

    public ContactController(AppDbContext context)
    {
        _context = context;
    }

    // Save Contact Message

    [HttpPost]
    public async Task<IActionResult> Create(ContactDto dto)
    {
        var contact = new ContactMessage
        {
            Name = dto.Name,
            Email = dto.Email,
            Subject = dto.Subject,
            Message = dto.Message
        };

        _context.ContactMessages.Add(contact);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            message = "Message received successfully"
        });
    }

    // Dashboard List

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var messages = await _context.ContactMessages
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return Ok(messages);
    }
}