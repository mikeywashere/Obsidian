namespace Obsidian.Web.Authorization;

/// <summary>
/// Defines the available roles in the application.
/// </summary>
public static class Roles
{
    /// <summary>
    /// System Administrator - Full system access including user management and system configuration.
    /// </summary>
    public const string SystemAdmin = "SystemAdmin";

    /// <summary>
    /// Administrator - Server management and configuration access.
    /// </summary>
    public const string Admin = "Admin";

    /// <summary>
    /// User - Basic access to view servers and logs.
    /// </summary>
    public const string User = "User";

    /// <summary>
    /// Gets all available roles.
    /// </summary>
    public static readonly string[] AllRoles = { SystemAdmin, Admin, User };
}
