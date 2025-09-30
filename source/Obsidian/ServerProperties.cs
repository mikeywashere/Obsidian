using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Obsidian
{
    /// <summary>
    /// Represents difficulty levels in Minecraft Bedrock server.
    /// </summary>
    public enum Difficulty
    {
        /// <summary>
        /// Peaceful difficulty - no hostile mobs spawn and hunger is disabled.
        /// </summary>
        Peaceful,
        
        /// <summary>
        /// Easy difficulty - mobs deal less damage and hunger depletes slowly.
        /// </summary>
        Easy,
        
        /// <summary>
        /// Normal difficulty - standard gameplay balance.
        /// </summary>
        Normal,
        
        /// <summary>
        /// Hard difficulty - mobs deal more damage and hunger depletes faster.
        /// </summary>
        Hard
    }

    /// <summary>
    /// Represents game modes in Minecraft Bedrock server.
    /// </summary>
    public enum GameMode
    {
        /// <summary>
        /// Survival mode - Players need to gather resources, manage hunger, and can take damage.
        /// </summary>
        Survival,
        
        /// <summary>
        /// Creative mode - Players have unlimited resources and can fly.
        /// </summary>
        Creative,
        
        /// <summary>
        /// Adventure mode - Players cannot break blocks directly, designed for custom maps.
        /// </summary>
        Adventure
    }

    internal class ServerProperties
    {
        /// <summary>
        /// The name of the server displayed to players.
        /// </summary>
        public string ServerName { get; set; } = "Dedicated Server";

        /// <summary>
        /// Game mode for new players.
        /// </summary>
        public GameMode GameMode { get; set; } = GameMode.Survival;

        /// <summary>
        /// Whether to force the gamemode specified in server properties.
        /// </summary>
        public bool ForceGameMode { get; set; } = false;

        /// <summary>
        /// World difficulty level.
        /// </summary>
        public Difficulty Difficulty { get; set; } = Difficulty.Easy;

        /// <summary>
        /// Whether cheats/commands are allowed.
        /// </summary>
        public bool AllowCheats { get; set; } = false;

        /// <summary>
        /// Maximum number of players that can join the server.
        /// </summary>
        public int MaxPlayers { get; set; } = 10;

        /// <summary>
        /// If true, players must authenticate to Xbox Live.
        /// </summary>
        public bool OnlineMode { get; set; } = true;

        /// <summary>
        /// If true, players must be in the allowlist.json file to join.
        /// </summary>
        public bool AllowList { get; set; } = false;

        /// <summary>
        /// The IPv4 port the server will listen on.
        /// </summary>
        public int ServerPort { get; set; } = 19132;

        /// <summary>
        /// The IPv6 port the server will listen on.
        /// </summary>
        public int ServerPortV6 { get; set; } = 19133;

        /// <summary>
        /// Whether server appears in LAN game lists.
        /// </summary>
        public bool EnableLanVisibility { get; set; } = true;

        /// <summary>
        /// Maximum view distance in chunks.
        /// </summary>
        public int ViewDistance { get; set; } = 32;

        /// <summary>
        /// Distance in chunks from players that the world will be ticked.
        /// </summary>
        public int TickDistance { get; set; } = 4;

        /// <summary>
        /// Minutes a player can idle before being kicked. 0 for no timeout.
        /// </summary>
        public int PlayerIdleTimeout { get; set; } = 30;

        /// <summary>
        /// Maximum threads the server will use.
        /// </summary>
        public int MaxThreads { get; set; } = 8;

        /// <summary>
        /// Name of the world/level.
        /// </summary>
        public string LevelName { get; set; } = "Bedrock level";

        /// <summary>
        /// Seed used to generate the world.
        /// </summary>
        public string LevelSeed { get; set; } = "";

        /// <summary>
        /// Default permission level for new players.
        /// Valid values: "visitor", "member", "operator"
        /// </summary>
        public string DefaultPlayerPermissionLevel { get; set; } = "member";

        /// <summary>
        /// Whether to require clients to use the server's texture packs.
        /// </summary>
        public bool TexturePackRequired { get; set; } = false;

        /// <summary>
        /// Whether to log content errors to a file.
        /// </summary>
        public bool ContentLogFileEnabled { get; set; } = false;

        /// <summary>
        /// Smallest size of raw network payload to compress.
        /// </summary>
        public int CompressionThreshold { get; set; } = 1;

        /// <summary>
        /// Compression algorithm for networking. Valid values: "zlib", "snappy"
        /// </summary>
        public string CompressionAlgorithm { get; set; } = "zlib";

        /// <summary>
        /// Whether to use strict server-authoritative movement.
        /// </summary>
        public bool ServerAuthoritativeMovementStrict { get; set; } = false;

        /// <summary>
        /// Whether to use strict server-authoritative dismount.
        /// </summary>
        public bool ServerAuthoritativeDismountStrict { get; set; } = false;

        /// <summary>
        /// Whether to use strict server-authoritative entity interactions.
        /// </summary>
        public bool ServerAuthoritativeEntityInteractionsStrict { get; set; } = false;

        /// <summary>
        /// Tolerance for discrepancies between client and server player positions.
        /// </summary>
        public float PlayerPositionAcceptanceThreshold { get; set; } = 0.5f;

        /// <summary>
        /// Threshold for difference between attack direction and look direction.
        /// </summary>
        public float PlayerMovementActionDirectionThreshold { get; set; } = 0.85f;

        /// <summary>
        /// Scalar for server-authoritative block breaking pick range.
        /// </summary>
        public float ServerAuthoritativeBlockBreakingPickRangeScalar { get; set; } = 1.5f;

        /// <summary>
        /// Chat restriction level. Valid values: "None", "Dropped", "Disabled"
        /// </summary>
        public string ChatRestriction { get; set; } = "None";

        /// <summary>
        /// Whether player interaction is disabled.
        /// </summary>
        public bool DisablePlayerInteraction { get; set; } = false;

        /// <summary>
        /// Whether client-side chunk generation is enabled.
        /// </summary>
        public bool ClientSideChunkGenerationEnabled { get; set; } = true;

        /// <summary>
        /// Whether to use hash-based block network IDs.
        /// </summary>
        public bool BlockNetworkIdsAreHashes { get; set; } = true;

        /// <summary>
        /// Whether persona features are disabled.
        /// </summary>
        public bool DisablePersona { get; set; } = false;

        /// <summary>
        /// Whether custom skins are disabled.
        /// </summary>
        public bool DisableCustomSkins { get; set; } = false;

        /// <summary>
        /// Server build radius ratio. "Disabled" or a value between 0.0 and 1.0.
        /// </summary>
        public string ServerBuildRadiusRatio { get; set; } = "Disabled";

        /// <summary>
        /// Whether outbound script debugging is allowed.
        /// </summary>
        public bool AllowOutboundScriptDebugging { get; set; } = false;

        /// <summary>
        /// Whether inbound script debugging is allowed.
        /// </summary>
        public bool AllowInboundScriptDebugging { get; set; } = false;

        /// <summary>
        /// Whether to enable UDP proxy for packet interception.
        /// </summary>
        public bool EnableUdpProxy { get; set; } = false;

        /// <summary>
        /// The port the UDP proxy should listen on.
        /// </summary>
        public int UdpProxyListenPort { get; set; } = 19134;

        /// <summary>
        /// The destination host to forward UDP packets to.
        /// </summary>
        public string UdpProxyDestinationHost { get; set; } = "127.0.0.1";

        /// <summary>
        /// The destination port to forward UDP packets to.
        /// </summary>
        public int UdpProxyDestinationPort { get; set; } = 19132;

        /// <summary>
        /// Converts a string to the corresponding Difficulty enum value.
        /// </summary>
        /// <param name="value">The difficulty as a string (case-insensitive)</param>
        /// <returns>The matching Difficulty enum value</returns>
        /// <exception cref="ArgumentException">Thrown when the string doesn't match any valid difficulty</exception>
        public static Difficulty ParseDifficulty(string value)
        {
            return value.ToLowerInvariant() switch
            {
                "peaceful" => Difficulty.Peaceful,
                "easy" => Difficulty.Easy,
                "normal" => Difficulty.Normal,
                "hard" => Difficulty.Hard,
                _ => throw new ArgumentException($"Invalid difficulty value: {value}", nameof(value))
            };
        }
        
        /// <summary>
        /// Converts a Difficulty enum value to its corresponding string representation.
        /// </summary>
        /// <param name="difficulty">The difficulty enum value</param>
        /// <returns>The string representation of the difficulty</returns>
        public static string DifficultyToString(Difficulty difficulty)
        {
            return difficulty.ToString().ToLowerInvariant();
        }

        /// <summary>
        /// Converts a string to the corresponding GameMode enum value.
        /// </summary>
        /// <param name="value">The game mode as a string (case-insensitive)</param>
        /// <returns>The matching GameMode enum value</returns>
        /// <exception cref="ArgumentException">Thrown when the string doesn't match any valid game mode</exception>
        public static GameMode ParseGameMode(string value)
        {
            return value.ToLowerInvariant() switch
            {
                "survival" => GameMode.Survival,
                "creative" => GameMode.Creative, 
                "adventure" => GameMode.Adventure,
                _ => throw new ArgumentException($"Invalid game mode value: {value}", nameof(value))
            };
        }
        
        /// <summary>
        /// Converts a GameMode enum value to its corresponding string representation.
        /// </summary>
        /// <param name="gameMode">The game mode enum value</param>
        /// <returns>The string representation of the game mode</returns>
        public static string GameModeToString(GameMode gameMode)
        {
            return gameMode.ToString().ToLowerInvariant();
        }

        /// <summary>
        /// Loads server properties from the specified file path.
        /// </summary>
        /// <param name="filePath">Path to the server.properties file.</param>
        /// <returns>A populated ServerProperties instance.</returns>
        /// <exception cref="FileNotFoundException">Thrown if the properties file doesn't exist.</exception>
        public static ServerProperties Load(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("Server properties file not found", filePath);

            var properties = new ServerProperties();
            var lines = File.ReadAllLines(filePath);
            
            foreach (var line in lines)
            {
                // Skip comments and empty lines
                if (string.IsNullOrWhiteSpace(line) || line.TrimStart().StartsWith('#'))
                    continue;
                    
                // Split the line at the equals sign
                var parts = line.Split(new[] { '=' }, 2);
                if (parts.Length != 2)
                    continue;
                    
                var key = parts[0].Trim();
                var value = parts[1].Trim();
                
                switch (key)
                {
                    case "server-name": properties.ServerName = value; break;
                    case "gamemode": properties.GameMode = ParseGameMode(value); break;
                    case "force-gamemode": properties.ForceGameMode = bool.Parse(value); break;
                    case "difficulty": properties.Difficulty = ParseDifficulty(value); break; // Using our enum parser
                    case "allow-cheats": properties.AllowCheats = bool.Parse(value); break;
                    case "max-players": properties.MaxPlayers = int.Parse(value); break;
                    case "online-mode": properties.OnlineMode = bool.Parse(value); break;
                    case "allow-list": properties.AllowList = bool.Parse(value); break;
                    case "server-port": properties.ServerPort = int.Parse(value); break;
                    case "server-portv6": properties.ServerPortV6 = int.Parse(value); break;
                    case "enable-lan-visibility": properties.EnableLanVisibility = bool.Parse(value); break;
                    case "view-distance": properties.ViewDistance = int.Parse(value); break;
                    case "tick-distance": properties.TickDistance = int.Parse(value); break;
                    case "player-idle-timeout": properties.PlayerIdleTimeout = int.Parse(value); break;
                    case "max-threads": properties.MaxThreads = int.Parse(value); break;
                    case "level-name": properties.LevelName = value; break;
                    case "level-seed": properties.LevelSeed = value; break;
                    case "default-player-permission-level": properties.DefaultPlayerPermissionLevel = value; break;
                    case "texturepack-required": properties.TexturePackRequired = bool.Parse(value); break;
                    case "content-log-file-enabled": properties.ContentLogFileEnabled = bool.Parse(value); break;
                    case "compression-threshold": properties.CompressionThreshold = int.Parse(value); break;
                    case "compression-algorithm": properties.CompressionAlgorithm = value; break;
                    case "server-authoritative-movement-strict": properties.ServerAuthoritativeMovementStrict = bool.Parse(value); break;
                    case "server-authoritative-dismount-strict": properties.ServerAuthoritativeDismountStrict = bool.Parse(value); break;
                    case "server-authoritative-entity-interactions-strict": properties.ServerAuthoritativeEntityInteractionsStrict = bool.Parse(value); break;
                    case "player-position-acceptance-threshold": properties.PlayerPositionAcceptanceThreshold = float.Parse(value); break;
                    case "player-movement-action-direction-threshold": properties.PlayerMovementActionDirectionThreshold = float.Parse(value); break;
                    case "server-authoritative-block-breaking-pick-range-scalar": properties.ServerAuthoritativeBlockBreakingPickRangeScalar = float.Parse(value); break;
                    case "chat-restriction": properties.ChatRestriction = value; break;
                    case "disable-player-interaction": properties.DisablePlayerInteraction = bool.Parse(value); break;
                    case "client-side-chunk-generation-enabled": properties.ClientSideChunkGenerationEnabled = bool.Parse(value); break;
                    case "block-network-ids-are-hashes": properties.BlockNetworkIdsAreHashes = bool.Parse(value); break;
                    case "disable-persona": properties.DisablePersona = bool.Parse(value); break;
                    case "disable-custom-skins": properties.DisableCustomSkins = bool.Parse(value); break;
                    case "server-build-radius-ratio": properties.ServerBuildRadiusRatio = value; break;
                    case "allow-outbound-script-debugging": properties.AllowOutboundScriptDebugging = bool.Parse(value); break;
                    case "allow-inbound-script-debugging": properties.AllowInboundScriptDebugging = bool.Parse(value); break;
                    case "enable-udp-proxy": properties.EnableUdpProxy = bool.Parse(value); break;
                    case "udp-proxy-listen-port": properties.UdpProxyListenPort = int.Parse(value); break;
                    case "udp-proxy-destination-host": properties.UdpProxyDestinationHost = value; break;
                    case "udp-proxy-destination-port": properties.UdpProxyDestinationPort = int.Parse(value); break;
                }
            }
            
            return properties;
        }
        
        /// <summary>
        /// Saves the current server properties to the specified file path.
        /// </summary>
        /// <param name="filePath">Path to the server.properties file.</param>
        public void Save(string filePath)
        {
            var lines = new List<string>
            {
                "server-name=" + ServerName,
                "gamemode=" + GameModeToString(GameMode),
                "force-gamemode=" + ForceGameMode.ToString().ToLower(),
                "difficulty=" + DifficultyToString(Difficulty), // Using our enum converter
                "allow-cheats=" + AllowCheats.ToString().ToLower(),
                "max-players=" + MaxPlayers,
                "online-mode=" + OnlineMode.ToString().ToLower(),
                "allow-list=" + AllowList.ToString().ToLower(),
                "server-port=" + ServerPort,
                "server-portv6=" + ServerPortV6,
                "enable-lan-visibility=" + EnableLanVisibility.ToString().ToLower(),
                "view-distance=" + ViewDistance,
                "tick-distance=" + TickDistance,
                "player-idle-timeout=" + PlayerIdleTimeout,
                "max-threads=" + MaxThreads,
                "level-name=" + LevelName,
                "level-seed=" + LevelSeed,
                "default-player-permission-level=" + DefaultPlayerPermissionLevel,
                "texturepack-required=" + TexturePackRequired.ToString().ToLower(),
                "content-log-file-enabled=" + ContentLogFileEnabled.ToString().ToLower(),
                "compression-threshold=" + CompressionThreshold,
                "compression-algorithm=" + CompressionAlgorithm,
                "server-authoritative-movement-strict=" + ServerAuthoritativeMovementStrict.ToString().ToLower(),
                "server-authoritative-dismount-strict=" + ServerAuthoritativeDismountStrict.ToString().ToLower(),
                "server-authoritative-entity-interactions-strict=" + ServerAuthoritativeEntityInteractionsStrict.ToString().ToLower(),
                "player-position-acceptance-threshold=" + PlayerPositionAcceptanceThreshold,
                "player-movement-action-direction-threshold=" + PlayerMovementActionDirectionThreshold,
                "server-authoritative-block-breaking-pick-range-scalar=" + ServerAuthoritativeBlockBreakingPickRangeScalar,
                "chat-restriction=" + ChatRestriction,
                "disable-player-interaction=" + DisablePlayerInteraction.ToString().ToLower(),
                "client-side-chunk-generation-enabled=" + ClientSideChunkGenerationEnabled.ToString().ToLower(),
                "block-network-ids-are-hashes=" + BlockNetworkIdsAreHashes.ToString().ToLower(),
                "disable-persona=" + DisablePersona.ToString().ToLower(),
                "disable-custom-skins=" + DisableCustomSkins.ToString().ToLower(),
                "server-build-radius-ratio=" + ServerBuildRadiusRatio,
                "allow-outbound-script-debugging=" + AllowOutboundScriptDebugging.ToString().ToLower(),
                "allow-inbound-script-debugging=" + AllowInboundScriptDebugging.ToString().ToLower(),
                "enable-udp-proxy=" + EnableUdpProxy.ToString().ToLower(),
                "udp-proxy-listen-port=" + UdpProxyListenPort,
                "udp-proxy-destination-host=" + UdpProxyDestinationHost,
                "udp-proxy-destination-port=" + UdpProxyDestinationPort
            };
            
            File.WriteAllLines(filePath, lines);
        }
    }
}
