namespace Obsidian.Models;

public class ServerLog
{
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
