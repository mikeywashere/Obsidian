using System.Net.Http.Json;
using Obsidian.Models;

namespace Obsidian.Web.Services;

public class HttpServerPlayerService : IServerPlayerService
{
    private readonly HttpClient _http;

    public HttpServerPlayerService(HttpClient http)
    {
        _http = http;
    }

    public async Task<IEnumerable<PlayerInfo>> GetPlayersAsync(string serverId)
        => await _http.GetFromJsonAsync<IEnumerable<PlayerInfo>>($"api/servers/{serverId}/players")
           ?? Enumerable.Empty<PlayerInfo>();
}
