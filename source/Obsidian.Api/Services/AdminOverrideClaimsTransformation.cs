using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Obsidian.DataAccess;
using System.Security.Claims;

namespace Obsidian.Api.Services;

/// <summary>
/// Enriches the ClaimsPrincipal with role claims sourced from the local
/// UserAdminOverrides table, allowing admins to be elevated without
/// requiring tenant-level Azure AD App Role assignment.
/// </summary>
public class AdminOverrideClaimsTransformation : IClaimsTransformation
{
    private readonly ObsidianDbContext _db;

    public AdminOverrideClaimsTransformation(ObsidianDbContext db)
    {
        _db = db;
    }

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        var objectId = principal.FindFirstValue("oid") ?? principal.FindFirstValue("sub");
        if (string.IsNullOrEmpty(objectId))
            return principal;

        var override_ = await _db.UserAdminOverrides.FindAsync(objectId);
        if (override_ == null)
            return principal;

        // Clone the identity to avoid mutating the original
        var identity = new ClaimsIdentity(principal.Identity);

        if (!principal.HasClaim(ClaimTypes.Role, override_.Role))
            identity.AddClaim(new Claim(ClaimTypes.Role, override_.Role));

        return new ClaimsPrincipal(identity);
    }
}
