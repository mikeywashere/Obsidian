using Obsidian.Models;

namespace Obsidian.Web.Services;

public interface IServerPropertiesService
{
    Task<ServerProperties?> GetPropertiesAsync(string serverId);
    Task SavePropertiesAsync(string serverId, ServerProperties properties);
}
