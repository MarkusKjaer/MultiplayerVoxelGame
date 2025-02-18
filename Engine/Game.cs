using System;
using CubeEngine.Engine.Entities;
using CubeEngine.Engine.MeshObject;
using CubeEngine.Engine.Window;
using OpenTK.Mathematics;

namespace CubeEngine.Engine
{
    public sealed class Game
    {
        public GameScene CurrentGameScene { get; private set; }


        public void Run()
        {
            CurrentGameScene = new("testGameScene");

            using CubeGameWindow gameWindow = new(CurrentGameScene);
            
            Camera camera = new(new(2f, 0f, -2f));
            CurrentGameScene.AddGameObject(camera);
            CurrentGameScene.ActiveCamera = camera;
            VisualGameObject visualGameObject = new();
            VisualGameObject visualGameObject2 = new();

            OBJFileReader oBJFileReader = new OBJFileReader();
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string parentDirectory = Directory.GetParent(baseDirectory).Parent.Parent.Parent.FullName;
            string objFilePath = Path.Combine(parentDirectory, "Models", "Suzanne.obj");

            MeshInfo meshInfo = oBJFileReader.ReadOBJFile(objFilePath);

            Material material = new(Path.Combine(parentDirectory, "Engine", "Window", "Shaders", "Cube.vert"), Path.Combine(parentDirectory, "Engine", "Window", "Shaders", "Cube.frag"), Path.Combine(parentDirectory, "Models", "ondskab.png"));

            visualGameObject.Mesh = new(meshInfo, material);
            visualGameObject2.Mesh = new(meshInfo, material);

            visualGameObject2.Rotation = new(40,140,40);
            visualGameObject2.Position = new(1, -2, 0);

            visualGameObject.Instantiate();
            visualGameObject2.Instantiate();

            gameWindow.Run();
        }
    }
}
