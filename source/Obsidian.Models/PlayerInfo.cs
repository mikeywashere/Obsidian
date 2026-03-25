namespace Obsidian.Models;

public record PlayerInfo(
    string ServerId,
    string Name,
    string Xuid,
    DateTime JoinedAt,
    DateTime LastSeen
);
