using System;
using System.Xml.Linq;
using CubeEngine.Engine.Entities;
using CubeEngine.Engine.MeshObject;
using CubeEngine.Engine.Window;
using CubeEngine.Engine.Window.Setup.Texture;
using OpenTK.Mathematics;

namespace CubeEngine.Engine
{
    public sealed class Game
    {
        public GameSceneWorld CurrentGameScene { get; private set; }

        public void Run()
        {
            CurrentGameScene = new("testGameScene");

            using CubeGameWindow gameWindow = new(CurrentGameScene);
            
            Camera camera = new(new(20f, 10f, -20f));
            CurrentGameScene.AddGameObject(camera);
            CurrentGameScene.ActiveCamera = camera;

            OBJFileReader oBJFileReader = new OBJFileReader();

            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string parentDirectory = Directory.GetParent(baseDirectory).Parent.Parent.Parent.FullName;
            string objFilePath = Path.Combine(parentDirectory, "Models", "Suzanne.obj");

            MeshInfo meshInfo = oBJFileReader.ReadOBJFile(objFilePath);

            TextureManager textureManager = new(Path.Combine(parentDirectory, "Models", "ondskab.png"));

            Material material = new(Path.Combine(parentDirectory, "Engine", "Window", "Shaders", "Cube.vert"), Path.Combine(parentDirectory, "Engine", "Window", "Shaders", "Cube.frag"), textureManager);

            TextureArrayManager textureArrayManagerForMap = LoadWorldTextures(parentDirectory);

            CurrentGameScene.Map = new(32, 1, textureArrayManagerForMap);


            VisualGameObject visualGameObject = new();
            visualGameObject.Mesh = new(meshInfo, material);
            visualGameObject.Instantiate();

            gameWindow.Run();
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
