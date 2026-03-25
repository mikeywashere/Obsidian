using Obsidian.Models;

namespace Obsidian.Api.Services;

public record PlayerEventArgs(string ServerId, PlayerInfo Player);

public interface IPlayerTracker
{
    event EventHandler<PlayerEventArgs>? PlayerJoined;
    event EventHandler<PlayerEventArgs>? PlayerLeft;
    IEnumerable<PlayerInfo> GetPlayers(string serverId);
}
