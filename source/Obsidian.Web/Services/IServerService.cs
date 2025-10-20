using Obsidian.Models;

namespace Obsidian.Web.Services;

public interface IServerService
{
    Task<List<ServerInfo>> GetServersAsync();
    Task<ServerInfo?> GetServerAsync(string id);
    Task<List<ServerLog>> GetServerLogsAsync(string serverId, int maxLines = 100);
    Task StartServerAsync(string serverId);
    Task StopServerAsync(string serverId);
}
