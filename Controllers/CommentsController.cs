using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillStoryBack.Data;
using SkillStoryBack.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace SkillStoryBack.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class CommentsController : ControllerBase
{
    private readonly AppDbContext _context;

    public CommentsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<Comment>>> GetComment()
        => await _context.Comments
                         .Include(c => c.User)
                         .Include(c => c.Skill)
                         .ToListAsync();

    [HttpGet("{id}")]
    [AllowAnonymous]

    public async Task<ActionResult> GetComment(
    string? search = null,
    string? sortBy = null,
    bool desc = false,
    int page = 1,
    int pageSize = 10)
    {
        var query = _context.Comments
            .Include(c => c.User)
            .Include(c => c.Skill)
            .AsQueryable();

        if (!string.IsNullOrEmpty(search))
            query = query.Where(c => c.Text.Contains(search) ||
                                     c.User.Username.Contains(search) ||
                                     c.Skill.Title.Contains(search));

        query = sortBy switch
        {
            "user" => desc ? query.OrderByDescending(c => c.User.Username) : query.OrderBy(c => c.User.Username),
            "skill" => desc ? query.OrderByDescending(c => c.Skill.Title) : query.OrderBy(c => c.Skill.Title),
            _ => query.OrderBy(c => c.Id)
        };

        var totalItems = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return Ok(new { totalItems, page, pageSize, items });
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<Comment>> PostComment(Comment comment)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetComment), new { id = comment.Id }, comment);
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> PutComment(int id, Comment comment)
    {
        if (id != comment.Id)
            return BadRequest();

        var existingComment = await _context.Comments.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
        if (existingComment == null)
            return NotFound();

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        if (existingComment.UserId != userId)
            return Forbid();

        _context.Entry(comment).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteComment(int id)
    {
        var comment = await _context.Comments.FindAsync(id);
        if (comment == null)
            return NotFound();

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        if (comment.UserId != userId)
            return Forbid();

        _context.Comments.Remove(comment);
        await _context.SaveChangesAsync();
        return NoContent();
    }

}
