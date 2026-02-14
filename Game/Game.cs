using CubeEngine.Engine.Client;
using CubeEngine.Engine.Client.Graphics.MeshObject;
using CubeEngine.Engine.Client.Graphics.Window;
using CubeEngine.Engine.Client.Graphics.Window.Setup.Texture;
using CubeEngine.Engine.Client.PlayerRender;
using CubeEngine.Engine.Entities;
using CubeEngine.Engine.Entities.Player;
using CubeEngine.Engine.Enum;
using CubeEngine.Engine.Server;
using CubeEngine.Util;
using MultiplayerVoxelGame.Game.Resources;
using OpenTK.Mathematics;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CubeEngine.Engine
{
    public sealed class Game
    {
        public GameSceneWorld CurrentGameScene { get; private set; }

        public GameClient? Client { get; private set; }

        public GameServer? Server { get; private set; }

        public async Task StartNetworkingAsync(NetworkRole role, string ip, int tcpPort, int udpPort)
        {
            if (role == NetworkRole.Server || role == NetworkRole.Host)
            {
                Noise.LoadHeightmap();

                Server = new GameServer(tcpPort, udpPort);
                await Server.StartAsync();
                Console.WriteLine($"Server started on TCP:{tcpPort} UDP:{udpPort}");
            }

            if (role == NetworkRole.Client || role == NetworkRole.Host)
            {
                Client = new GameClient(ip, tcpPort, udpPort);
                await Client.StartAsync();
                Console.WriteLine($"Client connecting to {ip}:{tcpPort}/{udpPort}");
            }
        }

        public void Run()
        {
            CurrentGameScene = new("testGameScene");

            CubeGameWindow gameWindow = new(CurrentGameScene);

            gameWindow.VSync = OpenTK.Windowing.Common.VSyncMode.On;
                
            PlayerCharacter player = new(new(80, 0, 80));
            PlayerCamera camera = new(new Vector3(0, 1.6f, 0), player)
            {
                Parent = player
            };

            player.Instantiate();
            camera.Instantiate();
            CurrentGameScene.ActiveCamera = camera;

            TextureArrayManager textureArrayManagerForMap = LoadWorldTextures();
            CurrentGameScene.Map = new(32, 64, 1, textureArrayManagerForMap);

            PlayerRenderManager playerRenderManager = new();

            playerRenderManager.Setup();
            playerRenderManager.Instantiate();

            gameWindow.Run();
        }

        private TextureArrayManager LoadWorldTextures()
        {
            XDocument xmlDoc = XDocument.Load(AssetsManager.Instance.LoadedAssets[("Textures", AssetType.XML)].FilePath);

            List<string> textureNames = new List<string>();

            foreach (var texture in xmlDoc.Descendants("texture"))
            {
                string filename = texture.Element("filename")?.Value ?? "";
                if (!string.IsNullOrEmpty(filename))
                {
                    string baseDir = AssetsManager.Instance.LoadedAssets[("Textures", AssetType.XML)].FilePath;
                    string parentDir = Directory.GetParent(baseDir).FullName;

                    textureNames.Add(Path.Combine(parentDir, filename));
                }
            }

            return new TextureArrayManager(textureNames.ToArray(), 1, 1);
        }
    }
}
