using Obsidian.Models;

namespace Obsidian.Api.Services;

public record ServerLogEventArgs(string ServerId, ServerLog Log);

public interface IServerManager
{
    event EventHandler<ServerLogEventArgs>? LogReceived;

    Task<IEnumerable<ServerInfo>> GetAllAsync();
    Task<ServerInfo?> GetAsync(string serverId);
    Task<IEnumerable<ServerLog>> GetLogsAsync(string serverId, int maxLines = 100);
    Task StartAsync(string serverId);
    Task StopAsync(string serverId);
    ServerInfo RegisterServer(string name, string installPath, int port = 19132);
    string? GetInstallPath(string serverId);
}
