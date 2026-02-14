using CubeEngine.Engine.Enum;
using MultiplayerVoxelGame.Game;
using MultiplayerVoxelGame.Game.Resources;
using System;
using System.Threading.Tasks;

namespace CubeEngine.Engine
{
    class Program
    {
        static async Task Main()
        {
            string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
            GameConfig config = GameConfig.Load(configPath);

            // Singleton instance of AssetsManager to load all assets at the start            
            _ = new AssetsManager(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets"));

            NetworkRole role = config.Role.ToLower() switch
            {
                "host" => NetworkRole.Host,
                "client" => NetworkRole.Client,
                "server" => NetworkRole.Server,
                _ => throw new Exception("Invalid role in config file.")
            };

            Game game = new();

            await game.StartNetworkingAsync(role, config.ServerIp, config.TcpPort, config.UdpPort);

            if (role == NetworkRole.Server)
            {
                Console.WriteLine("Server running. Press Enter to exit.");
                Console.ReadLine();
            }
            else
            {
                game.Run();
            }
        }
    }
}
