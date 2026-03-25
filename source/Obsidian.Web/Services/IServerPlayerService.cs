using Obsidian.Models;

namespace Obsidian.Web.Services;

public interface IServerPlayerService
{
    Task<IEnumerable<PlayerInfo>> GetPlayersAsync(string serverId);
}
