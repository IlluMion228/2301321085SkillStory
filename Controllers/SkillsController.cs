using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillStoryBack.Data;
using SkillStoryBack.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace SkillStoryBack.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SkillsController : ControllerBase
{
    private readonly AppDbContext _context;

    public SkillsController(AppDbContext context)
    {
        _context = context;
    }
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Skill>>> GetSkill()
        => await _context.Skills.Include(s => s.User).ToListAsync();

    [HttpGet("{id}")]
    public async Task<ActionResult<IEnumerable<Skill>>> GetSkill(
        string? search = null,
        string? sortBy = null,
        bool desc = false,
        int page = 1,
        int pageSize = 10)
    {
        var query = _context.Skills.Include(s => s.User).AsQueryable();

        if (!string.IsNullOrEmpty(search))
        {
            query = query.Where(s =>
                s.Title.Contains(search) ||
                (s.Description != null && s.Description.Contains(search)));
        }

        query = sortBy switch
        {
            "title" => desc ? query.OrderByDescending(s => s.Title) : query.OrderBy(s => s.Title),
            "level" => desc ? query.OrderByDescending(s => s.Level) : query.OrderBy(s => s.Level),
            "user" => desc ? query.OrderByDescending(s => s.User!.Username) : query.OrderBy(s => s.User!.Username),
            _ => query.OrderBy(s => s.Id)
        };

        var totalItems = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return Ok(new
        {
            totalItems,
            page,
            pageSize,
            items
        });
    }

    [HttpPost]
    public async Task<ActionResult<Skill>> PostSkill(Skill skill)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        _context.Skills.Add(skill);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetSkill), new { id = skill.Id }, skill);
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> PutSkill(int id, Skill skill)
    {
        if (id != skill.Id)
            return BadRequest();

        var existingSkill = await _context.Skills.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
        if (existingSkill == null)
            return NotFound();

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        if (existingSkill.UserId != userId)
            return Forbid();

        _context.Entry(skill).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSkill(int id)
    {
        var skill = await _context.Skills.FindAsync(id);
        if (skill == null)
            return NotFound();

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        if (skill.UserId != userId)
            return Forbid();

        _context.Skills.Remove(skill);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
