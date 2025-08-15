using CubeEngine.Engine.Client.Graphics.Window;
using CubeEngine.Engine.Client.World;

namespace CubeEngine.Engine
{
    public class GameSceneWorld(string name) : GameScene(name)
    {
        public Map Map { get; set; }

        public override void Update()
        {
            base.Update();
            Map?.UpdateMeshs(CubeGameWindow.Instance.CurrentGameScene.ActiveCamera, CubeGameWindow.Instance.WindowWidth, CubeGameWindow.Instance.Windowheight);
        }

        public override void Render()
        {
            
            base.Render();
            Map?.Render();
        }
    }
}
