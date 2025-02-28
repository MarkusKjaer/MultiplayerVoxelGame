
using CubeEngine.Engine.Entities.World;
using CubeEngine.Engine.Window;

namespace CubeEngine.Engine
{
    public class GameSceneWorld : GameScene
    {
        public GameSceneWorld(string name) : base(name) { }

        public Map Map { get; set; }

        public override void Update()
        {
            base.Update();
            Map.UpdateMeshs(CubeGameWindow.Instance.CurrentGameScene.ActiveCamera, CubeGameWindow.Instance.WindowWidth, CubeGameWindow.Instance.Windowheight);
        }

        public override void Render()
        {
            
            base.Render();
            Map.Render();
        }
    }
}
