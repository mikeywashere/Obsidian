using Obsidian.Models;

namespace Obsidian.Web.Services;

public class MockServerService : IServerService
{
    private readonly List<ServerInfo> _servers;
    private readonly Dictionary<string, List<ServerLog>> _logs;

    public MockServerService()
    {
        _servers = new List<ServerInfo>
        {
            new ServerInfo
            {
                Id = "server-1",
                Name = "Survival Server",
                Status = ServerStatus.Running,
                Version = "1.20.51",
                Port = 19132,
                MaxPlayers = 10,
                CurrentPlayers = 5,
                CreatedDate = DateTime.Now.AddDays(-30),
                LastStarted = DateTime.Now.AddHours(-2)
            },
            new ServerInfo
            {
                Id = "server-2",
                Name = "Creative Build Server",
                Status = ServerStatus.Stopped,
                Version = "1.20.51",
                Port = 19133,
                MaxPlayers = 20,
                CurrentPlayers = 0,
                CreatedDate = DateTime.Now.AddDays(-15),
                LastStarted = DateTime.Now.AddDays(-1)
            },
            new ServerInfo
            {
                Id = "server-3",
                Name = "Adventure World",
                Status = ServerStatus.Running,
                Version = "1.20.50",
                Port = 19134,
                MaxPlayers = 15,
                CurrentPlayers = 8,
                CreatedDate = DateTime.Now.AddDays(-60),
                LastStarted = DateTime.Now.AddHours(-5)
            },
            new ServerInfo
            {
                Id = "server-4",
                Name = "Test Server",
                Status = ServerStatus.Error,
                Version = "1.20.51",
                Port = 19135,
                MaxPlayers = 5,
                CurrentPlayers = 0,
                CreatedDate = DateTime.Now.AddDays(-7),
                LastStarted = DateTime.Now.AddMinutes(-30)
            }
        };

        _logs = new Dictionary<string, List<ServerLog>>();
        
        foreach (var server in _servers)
        {
            _logs[server.Id] = GenerateMockLogs(server);
        }
    }

    private List<ServerLog> GenerateMockLogs(ServerInfo server)
    {
        var logs = new List<ServerLog>();
        var now = DateTime.Now;

        if (server.Status == ServerStatus.Running)
        {
            logs.Add(new ServerLog { Timestamp = now.AddMinutes(-5), Level = Models.LogLevel.Info, Message = $"Server '{server.Name}' started successfully" });
            logs.Add(new ServerLog { Timestamp = now.AddMinutes(-4), Level = Models.LogLevel.Info, Message = "Loading world data..." });
            logs.Add(new ServerLog { Timestamp = now.AddMinutes(-3), Level = Models.LogLevel.Info, Message = "World loaded successfully" });
            logs.Add(new ServerLog { Timestamp = now.AddMinutes(-2), Level = Models.LogLevel.Info, Message = $"Server listening on port {server.Port}" });
            logs.Add(new ServerLog { Timestamp = now.AddMinutes(-1), Level = Models.LogLevel.Info, Message = "Player 'Steve' connected" });
            logs.Add(new ServerLog { Timestamp = now, Level = Models.LogLevel.Info, Message = "Server running normally" });
        }
        else if (server.Status == ServerStatus.Error)
        {
            logs.Add(new ServerLog { Timestamp = now.AddMinutes(-5), Level = Models.LogLevel.Info, Message = $"Server '{server.Name}' starting..." });
            logs.Add(new ServerLog { Timestamp = now.AddMinutes(-4), Level = Models.LogLevel.Error, Message = "Failed to bind to port: Address already in use" });
            logs.Add(new ServerLog { Timestamp = now.AddMinutes(-3), Level = Models.LogLevel.Error, Message = "Server startup failed" });
        }
        else
        {
            logs.Add(new ServerLog { Timestamp = now.AddHours(-1), Level = Models.LogLevel.Info, Message = $"Server '{server.Name}' stopped" });
        }

        return logs;
    }

    public Task<List<ServerInfo>> GetServersAsync()
    {
        return Task.FromResult(_servers);
    }

    public Task<ServerInfo?> GetServerAsync(string id)
    {
        var server = _servers.FirstOrDefault(s => s.Id == id);
        return Task.FromResult(server);
    }

    public Task<List<ServerLog>> GetServerLogsAsync(string serverId, int maxLines = 100)
    {
        if (_logs.TryGetValue(serverId, out var logs))
        {
            return Task.FromResult(logs.TakeLast(maxLines).ToList());
        }
        return Task.FromResult(new List<ServerLog>());
    }

    public async Task StartServerAsync(string serverId)
    {
        var server = _servers.FirstOrDefault(s => s.Id == serverId);
        if (server != null)
        {
            server.Status = ServerStatus.Starting;
            await Task.Delay(1000); // Simulate startup delay
            server.Status = ServerStatus.Running;
            server.LastStarted = DateTime.Now;
            
            // Add log entry
            if (_logs.TryGetValue(serverId, out var logs))
            {
                logs.Add(new ServerLog 
                { 
                    Timestamp = DateTime.Now, 
                    Level = Models.LogLevel.Info, 
                    Message = $"Server '{server.Name}' started" 
                });
            }
        }
    }

    public async Task StopServerAsync(string serverId)
    {
        var server = _servers.FirstOrDefault(s => s.Id == serverId);
        if (server != null)
        {
            server.Status = ServerStatus.Stopping;
            await Task.Delay(1000); // Simulate shutdown delay
            server.Status = ServerStatus.Stopped;
            server.CurrentPlayers = 0;
            
            // Add log entry
            if (_logs.TryGetValue(serverId, out var logs))
            {
                logs.Add(new ServerLog 
                { 
                    Timestamp = DateTime.Now, 
                    Level = Models.LogLevel.Info, 
                    Message = $"Server '{server.Name}' stopped" 
                });
            }
        }
    }
}
