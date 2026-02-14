using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;


namespace MultiplayerVoxelGame.Game
{

    public class GameConfig
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = "host";

        [JsonPropertyName("serverIp")]
        public string ServerIp { get; set; } = "localhost";

        [JsonPropertyName("tcpPort")]
        public int TcpPort { get; set; } = 8000;

        [JsonPropertyName("udpPort")]
        public int UdpPort { get; set; } = 9000;

        public static GameConfig Load(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine("Config file not found, using defaults.");
                return new GameConfig();
            }

            string json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<GameConfig>(json) ?? new GameConfig();
        }
    }

}
