using System.Xml.Linq;
using CubeEngine.Engine.Client;
using CubeEngine.Engine.Client.Graphics.MeshObject;
using CubeEngine.Engine.Client.Graphics.Window;
using CubeEngine.Engine.Client.Graphics.Window.Setup.Texture;
using CubeEngine.Engine.Entities;
using CubeEngine.Engine.Enum;
using CubeEngine.Engine.Server;
using CubeEngine.Util;
using OpenTK.Mathematics;

namespace CubeEngine.Engine
{
    public sealed class Game
    {
        public GameSceneWorld CurrentGameScene { get; private set; }

        public GameClient? Client { get; private set; }

        public GameServer? Server { get; private set; }

        public void Run(NetworkRole networkRole)
        {
            CurrentGameScene = new("testGameScene");

            CubeGameWindow? gameWindow = null;

            if (networkRole != NetworkRole.Server)
            {
                gameWindow = new(CurrentGameScene);

                gameWindow.VSync = OpenTK.Windowing.Common.VSyncMode.On;
            }
            if (networkRole == NetworkRole.Server || networkRole == NetworkRole.Host)
            {
                Server = new GameServer(8000, 9000);
                Server.Start();
            }

            if (networkRole == NetworkRole.Client || networkRole == NetworkRole.Host)
            {
                // Small delay to ensure server is listening
                Task.Delay(20).Wait();
                Client = new GameClient("localhost", 8000, 9000);
                Client.Start();
            }



            PlayerFlyControllerClient player = new();
            Camera camera = new(new Vector3(0, 0, 10))
            {
                Parent = player
            };

            CurrentGameScene.AddGameObject(player);
            CurrentGameScene.AddGameObject(camera);
            CurrentGameScene.ActiveCamera = camera;

            OBJFileReader oBJFileReader = new OBJFileReader();

            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string parentDirectory = Directory.GetParent(baseDirectory).Parent.Parent.Parent.FullName;
            string objFilePath = Path.Combine(parentDirectory, "Models", "Suzanne.obj");

            Noise.LoadHeightmap(Path.Combine(parentDirectory, "Util", "NoiseImage", "perlin.png"));


            MeshInfo meshInfo = oBJFileReader.ReadOBJFile(objFilePath);

            TextureManager textureManager = new(Path.Combine(parentDirectory, "Models", "ondskab.png"));

            Material material = new(Path.Combine(parentDirectory, "Engine", "Client", "Graphics", "Window", "Shaders", "Cube.vert"), Path.Combine(parentDirectory, "Engine", "Client", "Graphics", "Window", "Shaders", "Cube.frag"), textureManager);

            TextureArrayManager textureArrayManagerForMap = LoadWorldTextures(parentDirectory);

            CurrentGameScene.Map = new(32, 1, textureArrayManagerForMap);


            VisualGameObject abe = new()
            {
                Mesh = new(meshInfo, material),
                Parent = player,
            };

            // Later, invert the orientation
            abe.Orientation = Quaternion.FromAxisAngle(Vector3.UnitY, MathHelper.DegreesToRadians(180f));

            abe.Instantiate();

            if (networkRole != NetworkRole.Server && gameWindow != null)
            {
                gameWindow.Run();
            }
        }

        private TextureArrayManager LoadWorldTextures(string parentDirectory)
        {
            // Construct the path to the world textures XML file
            string pathToWorldTextures = Path.Combine(parentDirectory, "Models", "WorldTexture", "textures.xml");

            // Load the XML file
            XDocument xmlDoc = XDocument.Load(pathToWorldTextures);

            List<string> textureNames = new List<string>();

            // Parse the XML and extract texture filenames
            foreach (var texture in xmlDoc.Descendants("texture"))
            {
                string filename = texture.Element("filename")?.Value ?? "";
                if (!string.IsNullOrEmpty(filename))
                {
                    textureNames.Add(Path.Combine(parentDirectory, "Models", "WorldTexture", filename));
                }
            }

            // Create and return the TextureArrayManager
            return new TextureArrayManager(textureNames.ToArray(), 1, 1);
        }
    }
}
