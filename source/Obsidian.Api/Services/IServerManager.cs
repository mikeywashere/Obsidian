using Obsidian.Models;

namespace Obsidian.Api.Services;

public interface IServerManager
{
    Task<IEnumerable<ServerInfo>> GetAllAsync();
    Task<ServerInfo?> GetAsync(string serverId);
    Task<IEnumerable<ServerLog>> GetLogsAsync(string serverId, int maxLines = 100);
    Task StartAsync(string serverId);
    Task StopAsync(string serverId);
    ServerInfo RegisterServer(string name, string installPath, int port = 19132);
}
