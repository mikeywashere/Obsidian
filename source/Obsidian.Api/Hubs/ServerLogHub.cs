using Microsoft.AspNetCore.SignalR;

namespace Obsidian.Api.Hubs;

public class ServerLogHub : Hub
{
    public async Task JoinServer(string serverId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, $"server-{serverId}");
    }

    public async Task LeaveServer(string serverId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"server-{serverId}");
    }
}
