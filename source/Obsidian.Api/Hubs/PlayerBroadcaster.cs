using Microsoft.AspNetCore.SignalR;
using Obsidian.Api.Services;

namespace Obsidian.Api.Hubs;

public class PlayerBroadcaster : IHostedService
{
    private readonly IPlayerTracker _playerTracker;
    private readonly IHubContext<PlayerHub> _hubContext;

    public PlayerBroadcaster(IPlayerTracker playerTracker, IHubContext<PlayerHub> hubContext)
    {
        _playerTracker = playerTracker;
        _hubContext = hubContext;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _playerTracker.PlayerJoined += OnPlayerJoined;
        _playerTracker.PlayerLeft += OnPlayerLeft;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _playerTracker.PlayerJoined -= OnPlayerJoined;
        _playerTracker.PlayerLeft -= OnPlayerLeft;
        return Task.CompletedTask;
    }

    private void OnPlayerJoined(object? sender, PlayerEventArgs e)
    {
        _ = _hubContext.Clients
            .Group($"server-{e.ServerId}")
            .SendAsync("PlayerJoined", e.Player);
    }

    private void OnPlayerLeft(object? sender, PlayerEventArgs e)
    {
        _ = _hubContext.Clients
            .Group($"server-{e.ServerId}")
            .SendAsync("PlayerLeft", e.Player);
    }
}
