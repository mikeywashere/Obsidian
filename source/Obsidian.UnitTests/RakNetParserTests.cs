using System.Buffers.Binary;
using System.Text;
using Shouldly;

namespace Obsidian.UnitTests;

public class RakNetParserTests
{
    #region Packet type identification

    [Fact]
    public void Parse_UnconnectedPing_ReturnsCorrectType()
    {
        var data = new byte[] { 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        var result = RakNetParser.Parse(data, PacketDirection.ClientToServer);
        result.PacketType.ShouldBe(RakNetPacketType.UnconnectedPing);
    }

    [Fact]
    public void Parse_UnconnectedPong_ReturnsCorrectType()
    {
        var data = BuildPongPacket("MCPE;Test;800;1.21.0;0;10;1;World;Survival;1;19132;19133");
        var result = RakNetParser.Parse(data, PacketDirection.ServerToClient);
        result.PacketType.ShouldBe(RakNetPacketType.UnconnectedPong);
    }

    [Fact]
    public void Parse_OpenConnectionRequest1_ReturnsCorrectType()
    {
        var data = new byte[] { 0x05, 0x00 };
        var result = RakNetParser.Parse(data, PacketDirection.ClientToServer);
        result.PacketType.ShouldBe(RakNetPacketType.OpenConnectionRequest1);
    }

    [Fact]
    public void Parse_OpenConnectionRequest2_ReturnsCorrectType()
    {
        var data = new byte[] { 0x07, 0x00 };
        var result = RakNetParser.Parse(data, PacketDirection.ClientToServer);
        result.PacketType.ShouldBe(RakNetPacketType.OpenConnectionRequest2);
    }

    [Fact]
    public void Parse_OpenConnectionReply1_ReturnsCorrectType()
    {
        var data = new byte[] { 0x06, 0x00 };
        var result = RakNetParser.Parse(data, PacketDirection.ServerToClient);
        result.PacketType.ShouldBe(RakNetPacketType.OpenConnectionReply1);
    }

    [Fact]
    public void Parse_OpenConnectionReply2_ReturnsCorrectType()
    {
        var data = new byte[] { 0x08, 0x00 };
        var result = RakNetParser.Parse(data, PacketDirection.ServerToClient);
        result.PacketType.ShouldBe(RakNetPacketType.OpenConnectionReply2);
    }

    [Fact]
    public void Parse_NewIncomingConnection_ReturnsCorrectType()
    {
        var data = new byte[] { 0x13, 0x00 };
        var result = RakNetParser.Parse(data, PacketDirection.ServerToClient);
        result.PacketType.ShouldBe(RakNetPacketType.NewIncomingConnection);
    }

    [Fact]
    public void Parse_DisconnectNotification_ReturnsCorrectType()
    {
        var data = new byte[] { 0x15, 0x00 };
        var result = RakNetParser.Parse(data, PacketDirection.ClientToServer);
        result.PacketType.ShouldBe(RakNetPacketType.DisconnectNotification);
    }

    [Fact]
    public void Parse_Ack_ReturnsCorrectType()
    {
        var data = new byte[] { 0xc0, 0x00 };
        var result = RakNetParser.Parse(data, PacketDirection.ClientToServer);
        result.PacketType.ShouldBe(RakNetPacketType.Ack);
    }

    [Fact]
    public void Parse_Nack_ReturnsCorrectType()
    {
        var data = new byte[] { 0xa0, 0x00 };
        var result = RakNetParser.Parse(data, PacketDirection.ClientToServer);
        result.PacketType.ShouldBe(RakNetPacketType.Nack);
    }

    [Fact]
    public void Parse_GamePacket_ReturnsCorrectType()
    {
        var data = new byte[] { 0xfe, 0x00 };
        var result = RakNetParser.Parse(data, PacketDirection.ClientToServer);
        result.PacketType.ShouldBe(RakNetPacketType.GamePacket);
    }

    [Fact]
    public void Parse_UnknownByte_ReturnsUnknownType()
    {
        var data = new byte[] { 0x99, 0x00 };
        var result = RakNetParser.Parse(data, PacketDirection.ClientToServer);
        result.PacketType.ShouldBe(RakNetPacketType.Unknown);
    }

    #endregion

    #region DataPacket range (0x80–0x8f)

    [Fact]
    public void Parse_DataPacket_0x80_ReturnsDataPacketType()
    {
        var data = new byte[] { 0x80, 0x00, 0x00, 0x00 };
        var result = RakNetParser.Parse(data, PacketDirection.ClientToServer);
        result.PacketType.ShouldBe(RakNetPacketType.DataPacket);
    }

    [Fact]
    public void Parse_DataPacket_0x84_ReturnsDataPacketType()
    {
        var data = new byte[] { 0x84, 0x00, 0x00, 0x00 };
        var result = RakNetParser.Parse(data, PacketDirection.ClientToServer);
        result.PacketType.ShouldBe(RakNetPacketType.DataPacket);
    }

    [Fact]
    public void Parse_DataPacket_0x8f_ReturnsDataPacketType()
    {
        var data = new byte[] { 0x8f, 0x00, 0x00, 0x00 };
        var result = RakNetParser.Parse(data, PacketDirection.ClientToServer);
        result.PacketType.ShouldBe(RakNetPacketType.DataPacket);
    }

    #endregion

    #region DataPacket sequence number extraction

    [Fact]
    public void Parse_DataPacket_ExtractsSequenceNumber()
    {
        // 3-byte little-endian at offset 1: 0x05, 0x00, 0x00 = 5
        var data = new byte[] { 0x84, 0x05, 0x00, 0x00, 0xAA, 0xBB, 0xCC };
        var result = RakNetParser.Parse(data, PacketDirection.ClientToServer);
        result.SequenceNumber.ShouldBe(5L);
    }

    [Fact]
    public void Parse_DataPacket_LargeSequenceNumber()
    {
        // 3-byte little-endian: 0xFF, 0xFF, 0x00 = 65535
        var data = new byte[] { 0x84, 0xFF, 0xFF, 0x00, 0x00 };
        var result = RakNetParser.Parse(data, PacketDirection.ClientToServer);
        result.SequenceNumber.ShouldBe(65535L);
    }

    #endregion

    #region UnconnectedPong MOTD parsing

    private const string TestMotd = "MCPE;My World;800;1.21.0;3;20;12345678;Survival World;Survival;1;19132;19133";

    [Fact]
    public void Parse_UnconnectedPong_ExtractsPlayerCount()
    {
        var data = BuildPongPacket(TestMotd);
        var result = RakNetParser.Parse(data, PacketDirection.ServerToClient);
        result.PlayerCount.ShouldBe(3);
    }

    [Fact]
    public void Parse_UnconnectedPong_ExtractsMaxPlayers()
    {
        var data = BuildPongPacket(TestMotd);
        var result = RakNetParser.Parse(data, PacketDirection.ServerToClient);
        result.MaxPlayers.ShouldBe(20);
    }

    [Fact]
    public void Parse_UnconnectedPong_ExtractsWorldName()
    {
        var data = BuildPongPacket(TestMotd);
        var result = RakNetParser.Parse(data, PacketDirection.ServerToClient);
        result.WorldName.ShouldBe("Survival World");
    }

    [Fact]
    public void Parse_UnconnectedPong_ExtractsGameMode()
    {
        var data = BuildPongPacket(TestMotd);
        var result = RakNetParser.Parse(data, PacketDirection.ServerToClient);
        result.GameMode.ShouldBe("Survival");
    }

    [Fact]
    public void Parse_UnconnectedPong_ExtractsServerMotd()
    {
        var data = BuildPongPacket(TestMotd);
        var result = RakNetParser.Parse(data, PacketDirection.ServerToClient);
        result.ServerMotd.ShouldNotBeNull();
        result.ServerMotd.ShouldContain(TestMotd);
    }

    #endregion

    #region Direction preservation

    [Fact]
    public void Parse_PreservesClientToServerDirection()
    {
        var data = new byte[] { 0x01, 0x00 };
        var result = RakNetParser.Parse(data, PacketDirection.ClientToServer);
        result.Direction.ShouldBe(PacketDirection.ClientToServer);
    }

    [Fact]
    public void Parse_PreservesServerToClientDirection()
    {
        var data = new byte[] { 0x01, 0x00 };
        var result = RakNetParser.Parse(data, PacketDirection.ServerToClient);
        result.Direction.ShouldBe(PacketDirection.ServerToClient);
    }

    #endregion

    #region Raw data preservation

    [Fact]
    public void Parse_PreservesRawData()
    {
        var data = new byte[] { 0x01, 0xAA, 0xBB, 0xCC };
        var result = RakNetParser.Parse(data, PacketDirection.ClientToServer);
        result.RawData.ShouldBe(data);
    }

    #endregion

    #region Defensive parsing (never throws)

    [Fact]
    public void Parse_EmptyArray_ReturnsUnknown()
    {
        var result = RakNetParser.Parse(new byte[0], PacketDirection.ClientToServer);
        result.PacketType.ShouldBe(RakNetPacketType.Unknown);
    }

    [Fact]
    public void Parse_NullEquivalent_TruncatedPong_DoesNotThrow()
    {
        // Build a pong packet then truncate it mid-MOTD
        var full = BuildPongPacket(TestMotd);
        var truncated = full[..40]; // cut off partway through the MOTD string
        var result = Should.NotThrow(() => RakNetParser.Parse(truncated, PacketDirection.ServerToClient));
        result.PacketType.ShouldBe(RakNetPacketType.UnconnectedPong);
    }

    [Fact]
    public void Parse_TruncatedDataPacket_DoesNotThrow()
    {
        // Only 2 bytes — not enough for a sequence number
        var data = new byte[] { 0x84, 0x05 };
        var result = Should.NotThrow(() => RakNetParser.Parse(data, PacketDirection.ClientToServer));
        result.PacketType.ShouldBe(RakNetPacketType.DataPacket);
        result.SequenceNumber.ShouldBeNull();
    }

    [Fact]
    public void Parse_MalformedMotd_DoesNotThrow()
    {
        // MOTD with no semicolons
        var result = Should.NotThrow(() => RakNetParser.Parse(BuildPongPacket("NoSemicolonsHere"), PacketDirection.ServerToClient));
        result.PacketType.ShouldBe(RakNetPacketType.UnconnectedPong);
    }

    #endregion

    #region Helpers

    private static readonly byte[] OfflineMessageIdMagic =
    [
        0x00, 0xff, 0xff, 0x00, 0xfe, 0xfe, 0xfe, 0xfe,
        0xfd, 0xfd, 0xfd, 0xfd, 0x12, 0x34, 0x56, 0x78,
    ];

    /// <summary>
    /// Builds a valid UnconnectedPong (0x1c) packet with the given MOTD string.
    /// Layout: [0]=0x1c [1-8]=sendTime [9-16]=serverGuid [17-32]=magic [33-34]=strLen (BE) [35+]=motd UTF-8
    /// </summary>
    private static byte[] BuildPongPacket(string motd)
    {
        var motdBytes = Encoding.UTF8.GetBytes(motd);
        var packet = new byte[35 + motdBytes.Length];

        packet[0] = 0x1c;
        // bytes[1..8] = sendTime (Int64 = 0L) — already zero-initialized
        // bytes[9..16] = serverGuid (12345678L)
        BinaryPrimitives.WriteInt64BigEndian(packet.AsSpan(9, 8), 12345678L);
        // bytes[17..32] = offline message ID magic
        OfflineMessageIdMagic.CopyTo(packet, 17);
        // bytes[33..34] = MOTD length as big-endian uint16
        BinaryPrimitives.WriteUInt16BigEndian(packet.AsSpan(33, 2), (ushort)motdBytes.Length);
        // bytes[35+] = MOTD UTF-8
        motdBytes.CopyTo(packet, 35);

        return packet;
    }

    #endregion
}
