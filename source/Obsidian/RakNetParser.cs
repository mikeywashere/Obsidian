using System.Buffers.Binary;
using System.Text;

namespace Obsidian;

/// <summary>
/// Parses raw UDP bytes into structured RakNet/Minecraft Bedrock packet representations.
/// All parsing is defensive — never throws, always returns something meaningful.
/// Post-login Minecraft game packets are AES-256-CFB8 encrypted and are NOT decrypted.
/// </summary>
public static class RakNetParser
{
    // Offline message ID magic: 0x00ffff00fefefefefdfdfdfd12345678 (appears at offset 1 in unconnected packets)
    private static readonly byte[] OfflineMessageId =
    [
        0x00, 0xff, 0xff, 0x00, 0xfe, 0xfe, 0xfe, 0xfe,
        0xfd, 0xfd, 0xfd, 0xfd, 0x12, 0x34, 0x56, 0x78,
    ];

    /// <summary>
    /// Parses a raw UDP payload into a <see cref="ParsedPacket"/>.
    /// Returns an Unknown packet if the data is empty, too short, or malformed.
    /// </summary>
    public static ParsedPacket Parse(byte[] data, PacketDirection direction)
    {
        try
        {
            return ParseInternal(data, direction);
        }
        catch
        {
            return new ParsedPacket
            {
                PacketType = RakNetPacketType.Unknown,
                Direction = direction,
                RawData = data,
            };
        }
    }

    private static ParsedPacket ParseInternal(byte[] data, PacketDirection direction)
    {
        if (data == null || data.Length == 0)
        {
            return new ParsedPacket
            {
                PacketType = RakNetPacketType.Unknown,
                Direction = direction,
                RawData = data ?? [],
            };
        }

        byte id = data[0];

        // DataPacket range 0x80–0x8f
        if (id >= 0x80 && id <= 0x8f)
        {
            return ParseDataPacket(data, direction);
        }

        return id switch
        {
            0x01 => SimplePacket(RakNetPacketType.UnconnectedPing, data, direction),
            0x1c => ParseUnconnectedPong(data, direction),
            0x05 => SimplePacket(RakNetPacketType.OpenConnectionRequest1, data, direction),
            0x06 => SimplePacket(RakNetPacketType.OpenConnectionReply1, data, direction),
            0x07 => SimplePacket(RakNetPacketType.OpenConnectionRequest2, data, direction),
            0x08 => SimplePacket(RakNetPacketType.OpenConnectionReply2, data, direction),
            0x13 => SimplePacket(RakNetPacketType.NewIncomingConnection, data, direction),
            0x15 => SimplePacket(RakNetPacketType.DisconnectNotification, data, direction),
            0xc0 => SimplePacket(RakNetPacketType.Ack, data, direction),
            0xa0 => SimplePacket(RakNetPacketType.Nack, data, direction),
            0xfe => new ParsedPacket
            {
                PacketType = RakNetPacketType.GamePacket,
                Direction = direction,
                RawData = data,
                IsEncrypted = false, // Cannot determine without tracking login state
            },
            _ => new ParsedPacket
            {
                PacketType = RakNetPacketType.Unknown,
                Direction = direction,
                RawData = data,
            },
        };
    }

    private static ParsedPacket SimplePacket(RakNetPacketType type, byte[] data, PacketDirection direction) =>
        new()
        {
            PacketType = type,
            Direction = direction,
            RawData = data,
        };

    /// <summary>
    /// DataPacket (0x80–0x8f): extracts the 3-byte little-endian sequence number at offset 1.
    /// </summary>
    private static ParsedPacket ParseDataPacket(byte[] data, PacketDirection direction)
    {
        long? seqNum = null;
        if (data.Length >= 4)
        {
            // Sequence number is 3 bytes little-endian at offset 1
            seqNum = data[1] | ((long)data[2] << 8) | ((long)data[3] << 16);
        }

        return new ParsedPacket
        {
            PacketType = RakNetPacketType.DataPacket,
            Direction = direction,
            RawData = data,
            SequenceNumber = seqNum,
        };
    }

    /// <summary>
    /// UnconnectedPong (0x1c): extracts MOTD fields.
    /// Layout: [0]=0x1c [1-8]=sendTime [9-16]=serverGuid [17-32]=magic [33-34]=strLen [35+]=motd
    /// MOTD format: MCPE;{displayName};{protocol};{version};{players};{maxPlayers};{guid};{world};{gameMode};{nintendo};{port4};{port6}
    /// </summary>
    private static ParsedPacket ParseUnconnectedPong(byte[] data, PacketDirection direction)
    {
        const int MotdLengthOffset = 33;
        const int MotdOffset = 35;

        string? motd = null;
        int? playerCount = null;
        int? maxPlayers = null;
        string? worldName = null;
        string? gameMode = null;

        if (data.Length >= MotdOffset)
        {
            ushort motdLength = BinaryPrimitives.ReadUInt16BigEndian(data.AsSpan(MotdLengthOffset, 2));
            if (data.Length >= MotdOffset + motdLength && motdLength > 0)
            {
                motd = Encoding.UTF8.GetString(data, MotdOffset, motdLength);
                (playerCount, maxPlayers, worldName, gameMode) = ParseMotdFields(motd);
            }
        }

        return new ParsedPacket
        {
            PacketType = RakNetPacketType.UnconnectedPong,
            Direction = direction,
            RawData = data,
            ServerMotd = motd,
            PlayerCount = playerCount,
            MaxPlayers = maxPlayers,
            WorldName = worldName,
            GameMode = gameMode,
        };
    }

    /// <summary>
    /// Splits MCPE MOTD string and extracts player/world/game mode fields.
    /// Format: MCPE;{displayName};{protocol};{version};{players};{maxPlayers};{guid};{world};{gameMode};...
    /// </summary>
    private static (int? playerCount, int? maxPlayers, string? worldName, string? gameMode) ParseMotdFields(string motd)
    {
        var parts = motd.Split(';');
        // parts[0]=MCPE, [1]=name, [2]=protocol, [3]=version, [4]=players, [5]=maxPlayers, [6]=guid, [7]=world, [8]=gameMode
        int? playerCount = parts.Length > 4 && int.TryParse(parts[4], out var pc) ? pc : null;
        int? maxPlayers = parts.Length > 5 && int.TryParse(parts[5], out var mp) ? mp : null;
        string? worldName = parts.Length > 7 ? parts[7] : null;
        string? gameMode = parts.Length > 8 ? parts[8] : null;
        return (playerCount, maxPlayers, worldName, gameMode);
    }
}
