using System;
using CubeEngine.Engine.Window;
using OpenTK.Mathematics;

namespace CubeEngine.Engine
{
    public sealed class Game
    {
        public void Run()
        {
            Camera camera = new Camera(new(1f, 1f, -1f));
            using CubeGameWindow gameWindow = new(camera);
            gameWindow.Run();
        }
    }
}
