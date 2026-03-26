using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Obsidian.DataAccess;
using Obsidian.Models;
using Obsidian.Models.Authorization;
using System.Security.Claims;

namespace Obsidian.Api.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Policy = Policies.RequireSystemAdmin)]
public class AdminController : ControllerBase
{
    private readonly ObsidianDbContext _db;

    public AdminController(ObsidianDbContext db)
    {
        _db = db;
    }

    /// <summary>Returns all local admin role overrides.</summary>
    [HttpGet("users")]
    public async Task<ActionResult<IEnumerable<UserAdminOverride>>> GetUsers()
    {
        var overrides = await _db.UserAdminOverrides.ToListAsync();
        return Ok(overrides);
    }

    /// <summary>Grants a local admin role override to an Azure AD user.</summary>
    [HttpPost("users")]
    public async Task<ActionResult<UserAdminOverride>> AddUser([FromBody] UserAdminOverride request)
    {
        if (string.IsNullOrWhiteSpace(request.ObjectId))
            return BadRequest(new { error = "ObjectId is required." });

        if (request.Role != Roles.Admin && request.Role != Roles.SystemAdmin)
            return BadRequest(new { error = $"Role must be '{Roles.Admin}' or '{Roles.SystemAdmin}'." });

        var existing = await _db.UserAdminOverrides.FindAsync(request.ObjectId);
        if (existing != null)
            return Conflict(new { error = $"Override already exists for object ID '{request.ObjectId}'." });

        var granterObjectId = User.FindFirstValue("oid") ?? User.FindFirstValue("sub") ?? string.Empty;

        var entry = new UserAdminOverride
        {
            ObjectId = request.ObjectId,
            DisplayName = request.DisplayName,
            Role = request.Role,
            GrantedAt = DateTime.UtcNow,
            GrantedBy = granterObjectId
        };

        _db.UserAdminOverrides.Add(entry);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetUsers), entry);
    }

    /// <summary>Removes a local admin role override.</summary>
    [HttpDelete("users/{objectId}")]
    public async Task<IActionResult> DeleteUser(string objectId)
    {
        var entry = await _db.UserAdminOverrides.FindAsync(objectId);
        if (entry == null)
            return NotFound(new { error = $"No override found for object ID '{objectId}'." });

        _db.UserAdminOverrides.Remove(entry);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
