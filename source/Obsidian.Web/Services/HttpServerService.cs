using System.Net.Http.Json;
using Obsidian.Models;

namespace Obsidian.Web.Services;

public class HttpServerService : IServerService
{
    private readonly HttpClient _http;

    public HttpServerService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<ServerInfo>> GetServersAsync()
        => await _http.GetFromJsonAsync<List<ServerInfo>>("api/servers") ?? [];

    public async Task<ServerInfo?> GetServerAsync(string id)
        => await _http.GetFromJsonAsync<ServerInfo?>($"api/servers/{id}");

    public async Task<List<ServerLog>> GetServerLogsAsync(string serverId, int maxLines = 100)
        => await _http.GetFromJsonAsync<List<ServerLog>>($"api/servers/{serverId}/logs?maxLines={maxLines}") ?? [];

    public async Task StartServerAsync(string serverId)
        => await _http.PostAsync($"api/servers/{serverId}/start", null);

    public async Task StopServerAsync(string serverId)
        => await _http.PostAsync($"api/servers/{serverId}/stop", null);
}
