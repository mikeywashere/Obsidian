using System.Net.Http.Json;
using Obsidian.Models;

namespace Obsidian.Web.Services;

public class HttpServerPropertiesService : IServerPropertiesService
{
    private readonly HttpClient _http;

    public HttpServerPropertiesService(HttpClient http)
    {
        _http = http;
    }

    public async Task<ServerProperties?> GetPropertiesAsync(string serverId)
        => await _http.GetFromJsonAsync<ServerProperties?>($"api/servers/{serverId}/properties");

    public async Task SavePropertiesAsync(string serverId, ServerProperties properties)
        => await _http.PutAsJsonAsync($"api/servers/{serverId}/properties", properties);
}
