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
public class UsersController : ControllerBase
{
    private readonly AppDbContext _context;

    public UsersController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetUser()
        => await _context.Users.ToListAsync();

    [HttpGet("{id}")]
    public async Task<ActionResult> GetUser(
    string? search = null,
    string? sortBy = null,
    bool desc = false,
    int page = 1,
    int pageSize = 10)
    {
        var query = _context.Users.AsQueryable();

        if (!string.IsNullOrEmpty(search))
            query = query.Where(u => u.Username.Contains(search) || (u.Email != null && u.Email.Contains(search)));

        query = sortBy switch
        {
            "username" => desc ? query.OrderByDescending(u => u.Username) : query.OrderBy(u => u.Username),
            "email" => desc ? query.OrderByDescending(u => u.Email) : query.OrderBy(u => u.Email),
            _ => query.OrderBy(u => u.Id)
        };

        var totalItems = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return Ok(new { totalItems, page, pageSize, items });
    }

    [HttpPost]
    public async Task<ActionResult<User>> PostUser(User user)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> PutUser(int id, User user)
    {
        if (id != user.Id)
            return BadRequest();

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        if (userId != id)
            return Forbid();

        _context.Entry(user).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return NotFound();

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        if (userId != id)
            return Forbid();

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return NoContent();
    }

}
