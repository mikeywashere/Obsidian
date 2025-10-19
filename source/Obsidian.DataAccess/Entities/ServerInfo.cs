namespace Obsidian.DataAccess.Entities;

public class ServerInfo
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public ServerStatus Status { get; set; }
    public string Version { get; set; } = string.Empty;
    public int Port { get; set; }
    public int MaxPlayers { get; set; }
    public int CurrentPlayers { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? LastStarted { get; set; }
}

public enum ServerStatus
{
    Stopped,
    Starting,
    Running,
    Stopping,
    Error
}
