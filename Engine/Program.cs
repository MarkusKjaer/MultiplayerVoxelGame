using CubeEngine.Engine.Enum;
using System;

namespace CubeEngine.Engine
{
    class Program
    {
        static void Main()
        {
            Console.WriteLine("Enter role: host, client, or server");
            string input = Console.ReadLine()?.Trim().ToLower();

            Game game = new();

            switch (input)
            {
                case "host":
                    game.StartNetworkingAsync(NetworkRole.Host);
                    game.Run();
                    break;
                case "client":
                    game.StartNetworkingAsync(NetworkRole.Client);
                    game.Run();
                    break;
                case "server":
                    game.StartNetworkingAsync(NetworkRole.Server);
                    Console.WriteLine("Server running. Press Enter to exit.");
                    Console.ReadLine();
                    break;
                default:
                    Console.WriteLine("Invalid role. Please enter 'host', 'client', or 'server'.");
                    break;
            }
        }
    }
}
