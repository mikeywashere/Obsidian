using Microsoft.AspNetCore.SignalR;
using Obsidian.Api.Services;

namespace Obsidian.Api.Hubs;

public class ServerLogBroadcaster : IHostedService
{
    private readonly IServerManager _serverManager;
    private readonly IHubContext<ServerLogHub> _hubContext;

    public ServerLogBroadcaster(IServerManager serverManager, IHubContext<ServerLogHub> hubContext)
    {
        _serverManager = serverManager;
        _hubContext = hubContext;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _serverManager.LogReceived += OnLogReceived;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _serverManager.LogReceived -= OnLogReceived;
        return Task.CompletedTask;
    }

    private void OnLogReceived(object? sender, ServerLogEventArgs e)
    {
        _ = _hubContext.Clients
            .Group($"server-{e.ServerId}")
            .SendAsync("ReceiveLog", e.Log);
    }
}
