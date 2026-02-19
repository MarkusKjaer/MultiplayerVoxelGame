using CubeEngine.Engine.Client.Graphics.Window;

namespace CubeEngine.Util
{
    public class Time
    {
        private CubeGameWindow _window;

        public static double DeltaTime { get; private set; }
        public static double GlobalTime { get; private set; } = 0;

        public Time(CubeGameWindow gameWindow)
        {
            _window = gameWindow;
            _window.OnNewDeltaTime += NewDeltaTime;
        }

        private void NewDeltaTime(double deltaTime)
        {
            DeltaTime = deltaTime;
            GlobalTime += deltaTime;
        }
    }
}
