using System;
using CubeEngine.Engine.Window;
using OpenTK.Mathematics;

namespace CubeEngine.Engine
{
    public sealed class Game
    {
        public void Run()
        {
            Camera camera = new Camera(new(2f, 0f, -2f));
            using CubeGameWindow gameWindow = new(camera);
            gameWindow.Run();
        }
    }
}
