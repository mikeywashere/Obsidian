using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Obsidian.Models;

namespace Obsidian.Api.Services;

public class ServerManager : IServerManager
{
    private readonly ConcurrentDictionary<string, ManagedServer> _servers = new();

    public ServerManager()
    {
        // Seed with one in-memory server for demo
        RegisterServer("My Bedrock Server", "C:\\bedrock", 19132);
    }

    public Task<IEnumerable<ServerInfo>> GetAllAsync()
    {
        var servers = _servers.Values.Select(s => s.Info).ToList();
        return Task.FromResult<IEnumerable<ServerInfo>>(servers);
    }

    public Task<ServerInfo?> GetAsync(string serverId)
    {
        _servers.TryGetValue(serverId, out var server);
        return Task.FromResult(server?.Info);
    }

    public Task<IEnumerable<ServerLog>> GetLogsAsync(string serverId, int maxLines = 100)
    {
        if (!_servers.TryGetValue(serverId, out var server))
        {
            return Task.FromResult<IEnumerable<ServerLog>>(Array.Empty<ServerLog>());
        }

        var logs = server.Logs.TakeLast(maxLines).ToList();
        return Task.FromResult<IEnumerable<ServerLog>>(logs);
    }

    public async Task StartAsync(string serverId)
    {
        if (!_servers.TryGetValue(serverId, out var server))
        {
            throw new InvalidOperationException($"Server '{serverId}' not found.");
        }

        if (server.Process != null && !server.Process.HasExited)
        {
            throw new InvalidOperationException($"Server '{serverId}' is already running.");
        }

        server.Info.Status = ServerStatus.Starting;

        var executableName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? "bedrock_server.exe"
            : "bedrock_server";

        var executablePath = Path.Combine(server.InstallPath, executableName);

        var startInfo = new ProcessStartInfo
        {
            FileName = executablePath,
            WorkingDirectory = server.InstallPath,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = true,
            CreateNoWindow = true
        };

        var process = new Process { StartInfo = startInfo };

        process.OutputDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                server.Logs.Add(new ServerLog
                {
                    Timestamp = DateTime.UtcNow,
                    Level = Models.LogLevel.Info,
                    Message = e.Data
                });
            }
        };

        process.ErrorDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                server.Logs.Add(new ServerLog
                {
                    Timestamp = DateTime.UtcNow,
                    Level = Models.LogLevel.Error,
                    Message = e.Data
                });
            }
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        server.Process = process;
        server.Info.Status = ServerStatus.Running;
        server.Info.LastStarted = DateTime.UtcNow;

        await Task.CompletedTask;
    }

    public async Task StopAsync(string serverId)
    {
        if (!_servers.TryGetValue(serverId, out var server))
        {
            throw new InvalidOperationException($"Server '{serverId}' not found.");
        }

        if (server.Process == null || server.Process.HasExited)
        {
            server.Info.Status = ServerStatus.Stopped;
            return;
        }

        server.Info.Status = ServerStatus.Stopping;

        try
        {
            // Send stop command to the server
            await server.Process.StandardInput.WriteLineAsync("stop");
            await server.Process.StandardInput.FlushAsync();

            // Wait for graceful shutdown (10 second timeout)
            var exited = await Task.Run(() => server.Process.WaitForExit(10000));

            if (!exited)
            {
                // Force kill if not exited gracefully
                server.Process.Kill();
                await Task.Run(() => server.Process.WaitForExit());
            }
        }
        catch (Exception ex)
        {
            server.Logs.Add(new ServerLog
            {
                Timestamp = DateTime.UtcNow,
                Level = Models.LogLevel.Error,
                Message = $"Error stopping server: {ex.Message}"
            });

            // Attempt to kill anyway
            try
            {
                server.Process.Kill();
            }
            catch { }
        }

        server.Info.Status = ServerStatus.Stopped;
        server.Process = null;
    }

    public ServerInfo RegisterServer(string name, string installPath, int port = 19132)
    {
        var id = $"server-{_servers.Count + 1}";
        var info = new ServerInfo
        {
            Id = id,
            Name = name,
            Status = ServerStatus.Stopped,
            Version = "Unknown",
            Port = port,
            MaxPlayers = 10,
            CurrentPlayers = 0,
            CreatedDate = DateTime.UtcNow
        };

        var server = new ManagedServer(info, installPath);
        _servers[id] = server;
        return info;
    }

    private class ManagedServer
    {
        public ServerInfo Info { get; }
        public string InstallPath { get; }
        public Process? Process { get; set; }
        public List<ServerLog> Logs { get; } = new();

        public ManagedServer(ServerInfo info, string installPath)
        {
            Info = info;
            InstallPath = installPath;
        }
    }
}
