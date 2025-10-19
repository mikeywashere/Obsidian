namespace Obsidian.DataAccess.Entities;

public class ServerLog
{
    public int Id { get; set; }
    public string ServerId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public LogLevel Level { get; set; }
    public string Message { get; set; } = string.Empty;
}

public enum LogLevel
{
    Debug,
    Info,
    Warning,
    Error
}
