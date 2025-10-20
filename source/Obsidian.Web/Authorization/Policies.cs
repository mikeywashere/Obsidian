namespace Obsidian.Web.Authorization;

/// <summary>
/// Defines authorization policy names used throughout the application.
/// </summary>
public static class Policies
{
    /// <summary>
    /// Policy that requires SystemAdmin role.
    /// </summary>
    public const string RequireSystemAdmin = "RequireSystemAdmin";

    /// <summary>
    /// Policy that requires Admin role (includes SystemAdmin).
    /// </summary>
    public const string RequireAdmin = "RequireAdmin";

    /// <summary>
    /// Policy that requires User role (includes Admin and SystemAdmin).
    /// </summary>
    public const string RequireUser = "RequireUser";
}
