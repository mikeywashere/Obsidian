using Obsidian.Models;

namespace Obsidian.Web.Services;

public interface IAdminService
{
    Task<IEnumerable<UserAdminOverride>> GetAdminUsersAsync();
    Task GrantAdminAsync(UserAdminOverride user);
    Task RevokeAdminAsync(string objectId);
}
