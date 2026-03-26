using System.Net.Http.Json;
using Obsidian.Models;

namespace Obsidian.Web.Services;

public class HttpAdminService : IAdminService
{
    private readonly HttpClient _http;

    public HttpAdminService(HttpClient http)
    {
        _http = http;
    }

    public async Task<IEnumerable<UserAdminOverride>> GetAdminUsersAsync()
        => await _http.GetFromJsonAsync<IEnumerable<UserAdminOverride>>("api/admin/users")
           ?? Enumerable.Empty<UserAdminOverride>();

    public async Task GrantAdminAsync(UserAdminOverride user)
        => await _http.PostAsJsonAsync("api/admin/users", user);

    public async Task RevokeAdminAsync(string objectId)
        => await _http.DeleteAsync($"api/admin/users/{Uri.EscapeDataString(objectId)}");
}
