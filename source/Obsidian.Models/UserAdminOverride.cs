namespace Obsidian.Models;

/// <summary>
/// Local DB override that grants an Azure AD user an elevated role,
/// bypassing the need for a tenant admin to configure App Roles in Azure AD.
/// </summary>
public class UserAdminOverride
{
    /// <summary>Azure AD object ID (oid claim) — primary key.</summary>
    public string ObjectId { get; set; } = string.Empty;

    /// <summary>Human-readable display name for the user.</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>Role granted: "Admin" or "SystemAdmin".</summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>UTC timestamp when the override was created.</summary>
    public DateTime GrantedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Object ID of the SystemAdmin who granted this override.</summary>
    public string GrantedBy { get; set; } = string.Empty;
}
