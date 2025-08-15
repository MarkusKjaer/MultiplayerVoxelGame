using CubeEngine.Engine.Client.Graphics.Window;

namespace CubeEngine.Util
{
    public class Time
    {
        private CubeGameWindow _window;

        public static double DeltaTime { get; set; }

        public Time(CubeGameWindow gameWindow)
        {
            _window = gameWindow;
            _window.OnNewDeltaTime += NewDeltaTime;
        }

        private void NewDeltaTime(double deltaTime)
        {
            DeltaTime = deltaTime;
        }
    }
}
