using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyDailyJ.Api.Data;
using MyDailyJ.Api.Dtos;
using MyDailyJ.Api.Models;

namespace MyDailyJ.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/entries")]
public class EntriesController : ControllerBase
{
    private readonly AppDbContext _db;

    public EntriesController(AppDbContext db)
    {
        _db = db;
    }

    private int CurrentUserId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);

    private static JournalEntryDto ToDto(JournalEntry entry) => new()
    {
        Id = entry.Id,
        UserId = entry.UserId,
        Title = entry.Title,
        Content = entry.Content,
        CreatedAt = entry.CreatedAt.ToString("o"),
        UpdatedAt = entry.UpdatedAt?.ToString("o")
    };

    [HttpGet]
    public async Task<ActionResult<List<JournalEntryDto>>> GetEntries()
    {
        var entries = await _db.JournalEntries
            .Where(e => e.UserId == CurrentUserId)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();

        return Ok(entries.Select(ToDto));
    }

    [HttpGet("search")]
    public async Task<ActionResult<List<JournalEntryDto>>> SearchEntries([FromQuery] string q)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return Ok(new List<JournalEntryDto>());
        }

        var entries = await _db.JournalEntries
            .Where(e => e.UserId == CurrentUserId &&
                        (EF.Functions.Like(e.Title, $"%{q}%") || EF.Functions.Like(e.Content, $"%{q}%")))
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();

        return Ok(entries.Select(ToDto));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<JournalEntryDto>> GetEntry(int id)
    {
        var entry = await _db.JournalEntries
            .SingleOrDefaultAsync(e => e.Id == id && e.UserId == CurrentUserId);

        if (entry == null)
        {
            return NotFound();
        }

        return Ok(ToDto(entry));
    }

    [HttpPost]
    public async Task<ActionResult<JournalEntryDto>> CreateEntry(JournalEntryDto request)
    {
        var entry = new JournalEntry
        {
            UserId = CurrentUserId,
            Title = request.Title,
            Content = request.Content,
            CreatedAt = DateTime.UtcNow
        };

        _db.JournalEntries.Add(entry);
        await _db.SaveChangesAsync();

        return Ok(ToDto(entry));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<JournalEntryDto>> UpdateEntry(int id, JournalEntryDto request)
    {
        var entry = await _db.JournalEntries
            .SingleOrDefaultAsync(e => e.Id == id && e.UserId == CurrentUserId);

        if (entry == null)
        {
            return NotFound();
        }

        entry.Title = request.Title;
        entry.Content = request.Content;
        entry.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return Ok(ToDto(entry));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteEntry(int id)
    {
        var entry = await _db.JournalEntries
            .SingleOrDefaultAsync(e => e.Id == id && e.UserId == CurrentUserId);

        if (entry == null)
        {
            return NotFound();
        }

        _db.JournalEntries.Remove(entry);
        await _db.SaveChangesAsync();

        return Ok();
    }
}
