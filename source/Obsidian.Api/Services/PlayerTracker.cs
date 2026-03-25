using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Obsidian.Models;

namespace Obsidian.Api.Services;

public class PlayerTracker : IPlayerTracker
{
    private static readonly Regex ConnectPattern =
        new(@"(?:\[INFO\]\s+)?Player connected:\s+(?<name>[^,]+),\s+xuid:\s+(?<xuid>\d+)",
            RegexOptions.Compiled);

    private static readonly Regex DisconnectPattern =
        new(@"(?:\[INFO\]\s+)?Player disconnected:\s+(?<name>[^,]+),\s+xuid:\s+(?<xuid>\d+)",
            RegexOptions.Compiled);

    // Outer key: serverId, inner key: xuid
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, PlayerInfo>> _players = new();

    public event EventHandler<PlayerEventArgs>? PlayerJoined;
    public event EventHandler<PlayerEventArgs>? PlayerLeft;

    public PlayerTracker(IServerManager serverManager)
    {
        serverManager.LogReceived += OnLogReceived;
    }

    public IEnumerable<PlayerInfo> GetPlayers(string serverId)
    {
        if (_players.TryGetValue(serverId, out var serverPlayers))
            return serverPlayers.Values.ToList();
        return Enumerable.Empty<PlayerInfo>();
    }

    private void OnLogReceived(object? sender, ServerLogEventArgs e)
    {
        var message = e.Log.Message;

        var connectMatch = ConnectPattern.Match(message);
        if (connectMatch.Success)
        {
            var name = connectMatch.Groups["name"].Value.Trim();
            var xuid = connectMatch.Groups["xuid"].Value;
            var now = DateTime.UtcNow;

            var serverPlayers = _players.GetOrAdd(e.ServerId, _ => new ConcurrentDictionary<string, PlayerInfo>());
            var player = new PlayerInfo(e.ServerId, name, xuid, now, now);
            serverPlayers[xuid] = player;

            PlayerJoined?.Invoke(this, new PlayerEventArgs(e.ServerId, player));
            return;
        }

        var disconnectMatch = DisconnectPattern.Match(message);
        if (disconnectMatch.Success)
        {
            var xuid = disconnectMatch.Groups["xuid"].Value;

            if (_players.TryGetValue(e.ServerId, out var serverPlayers) &&
                serverPlayers.TryRemove(xuid, out var player))
            {
                PlayerLeft?.Invoke(this, new PlayerEventArgs(e.ServerId, player));
            }
        }
    }
}
