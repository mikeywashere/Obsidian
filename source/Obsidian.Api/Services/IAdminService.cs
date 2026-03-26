using Obsidian.Models;

namespace Obsidian.Api.Services;

// TODO: remove stub once Neo ships IAdminService
public interface IAdminService
{
    Task<IEnumerable<UserAdminOverride>> GetAdminUsersAsync();
    Task GrantAdminAsync(string oid);
    Task<bool> RevokeAdminAsync(string oid);
    Task<UserAdminOverride?> GetAdminOverrideAsync(string oid);
}
