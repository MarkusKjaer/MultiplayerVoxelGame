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
                    game.Run(NetworkRole.Host);
                    break;
                case "client":
                    game.Run(NetworkRole.Client);
                    break;
                case "server":
                    game.Run(NetworkRole.Server);
                    break;
                default:
                    Console.WriteLine("Invalid role. Please enter 'host', 'client', or 'server'.");
                    break;
            }
        }
    }
}
