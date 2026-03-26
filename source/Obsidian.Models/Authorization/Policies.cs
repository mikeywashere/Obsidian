namespace Obsidian.Models.Authorization;

/// <summary>
/// Defines authorization policy names used throughout the application.
/// </summary>
public static class Policies
{
    /// <summary>
    /// Policy that requires the SystemAdmin role.
    /// </summary>
    public const string RequireSystemAdmin = "RequireSystemAdmin";

    /// <summary>
    /// Policy that requires Admin or SystemAdmin role.
    /// </summary>
    public const string RequireAdmin = "RequireAdmin";

    /// <summary>
    /// Policy that requires User, Admin, or SystemAdmin role.
    /// </summary>
    public const string RequireUser = "RequireUser";
}
