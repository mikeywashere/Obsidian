namespace Obsidian.Models.Authorization;

/// <summary>
/// Defines the available roles in the application.
/// </summary>
public static class Roles
{
    /// <summary>
    /// System Administrator — full system access including user management.
    /// </summary>
    public const string SystemAdmin = "SystemAdmin";

    /// <summary>
    /// Administrator — server management and configuration access.
    /// </summary>
    public const string Admin = "Admin";

    /// <summary>
    /// User — basic read-only access to view servers and logs.
    /// </summary>
    public const string User = "User";

    /// <summary>
    /// Gets all available roles.
    /// </summary>
    public static readonly string[] AllRoles = [SystemAdmin, Admin, User];
}
