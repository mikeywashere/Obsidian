using System.IO;

namespace Obsidian.Models;
    public enum Difficulty
    {
        Peaceful,
        
        Easy,
        
        Normal,
        
        Hard
    }

    public enum GameMode
    {
        Survival,
        
        Creative,
        
        Adventure
    }

    public class ServerProperties
    {
        public string ServerName { get; set; } = "Dedicated Server";

        public GameMode GameMode { get; set; } = GameMode.Survival;

        public bool ForceGameMode { get; set; } = false;

        public Difficulty Difficulty { get; set; } = Difficulty.Easy;

        public bool AllowCheats { get; set; } = false;

        public int MaxPlayers { get; set; } = 10;

        public bool OnlineMode { get; set; } = true;

        public bool AllowList { get; set; } = false;

        public int ServerPort { get; set; } = 19132;

        public int ServerPortV6 { get; set; } = 19133;

        public bool EnableLanVisibility { get; set; } = true;

        public int ViewDistance { get; set; } = 32;

        public int TickDistance { get; set; } = 4;

        public int PlayerIdleTimeout { get; set; } = 30;

        public int MaxThreads { get; set; } = 8;

        public string LevelName { get; set; } = "Bedrock level";

        public string LevelSeed { get; set; } = "";

        public string DefaultPlayerPermissionLevel { get; set; } = "member";

        public bool TexturePackRequired { get; set; } = false;

        public bool ContentLogFileEnabled { get; set; } = false;

        public int CompressionThreshold { get; set; } = 1;

        public string CompressionAlgorithm { get; set; } = "zlib";

        public bool ServerAuthoritativeMovementStrict { get; set; } = false;

        public bool ServerAuthoritativeDismountStrict { get; set; } = false;

        public bool ServerAuthoritativeEntityInteractionsStrict { get; set; } = false;

        public float PlayerPositionAcceptanceThreshold { get; set; } = 0.5f;

        public float PlayerMovementActionDirectionThreshold { get; set; } = 0.85f;

        public float ServerAuthoritativeBlockBreakingPickRangeScalar { get; set; } = 1.5f;

        public string ChatRestriction { get; set; } = "None";

        public bool DisablePlayerInteraction { get; set; } = false;

        public bool ClientSideChunkGenerationEnabled { get; set; } = true;

        public bool BlockNetworkIdsAreHashes { get; set; } = true;

        public bool DisablePersona { get; set; } = false;

        public bool DisableCustomSkins { get; set; } = false;

        public string ServerBuildRadiusRatio { get; set; } = "Disabled";

        public bool AllowOutboundScriptDebugging { get; set; } = false;

        public bool AllowInboundScriptDebugging { get; set; } = false;

        public bool EnableUdpProxy { get; set; } = false;

        public int UdpProxyListenPort { get; set; } = 19134;

        public string UdpProxyDestinationHost { get; set; } = "127.0.0.1";

        public int UdpProxyDestinationPort { get; set; } = 19132;

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
        
        public static string DifficultyToString(Difficulty difficulty)
        {
            return difficulty.ToString().ToLowerInvariant();
        }

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
        
        public static string GameModeToString(GameMode gameMode)
        {
            return gameMode.ToString().ToLowerInvariant();
        }

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

