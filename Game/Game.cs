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

        public async Task StartNetworkingAsync(NetworkRole role)
        {
            if (role == NetworkRole.Server || role == NetworkRole.Host)
            {
                string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string parentDirectory = Directory.GetParent(baseDirectory).Parent.Parent.Parent.FullName;
                Noise.LoadHeightmap(Path.Combine(parentDirectory, "Util", "NoiseImage", "perlin.png"));

                Server = new GameServer(8000, 9000);
                await Server.StartAsync();
            }

            if (role == NetworkRole.Client || role == NetworkRole.Host)
            {
                Client = new GameClient("localhost", 8000, 9000);
                Client.StartAsync();
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

            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string parentDirectory = Directory.GetParent(baseDirectory).Parent.Parent.Parent.FullName;

            TextureArrayManager textureArrayManagerForMap = LoadWorldTextures(parentDirectory);
            CurrentGameScene.Map = new(32, 64, 1, textureArrayManagerForMap);

            PlayerRenderManager playerRenderManager = new();

            playerRenderManager.Setup();
            playerRenderManager.Instantiate();

            gameWindow.Run();
        }

        private TextureArrayManager LoadWorldTextures(string parentDirectory)
        {
            string pathToWorldTextures = Path.Combine(parentDirectory, "Models", "WorldTexture", "textures.xml");

            XDocument xmlDoc = XDocument.Load(pathToWorldTextures);

            List<string> textureNames = new List<string>();

            foreach (var texture in xmlDoc.Descendants("texture"))
            {
                string filename = texture.Element("filename")?.Value ?? "";
                if (!string.IsNullOrEmpty(filename))
                {
                    textureNames.Add(Path.Combine(parentDirectory, "Models", "WorldTexture", filename));
                }
            }

            return new TextureArrayManager(textureNames.ToArray(), 1, 1);
        }
    }
}
