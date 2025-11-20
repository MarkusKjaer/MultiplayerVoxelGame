using CubeEngine.Engine.Client.Graphics.MeshObject;
using CubeEngine.Engine.Client.Graphics.Window;
using OpenTK.Mathematics;

namespace CubeEngine.Engine.Entities
{
    public class VisualGameObject : GameObject
    {
        public Mesh Mesh { get; set; }

        public override void OnLoad()
        {
            base.OnLoad();

            Mesh.Load();
        }

        public override void OnUnload()
        {
            base.OnUnload();

            Mesh.Unload();
        }

        public override void OnUpdate()
        {
            Mesh.Model = Matrix4.CreateFromQuaternion(GlobalOrientation) * Matrix4.CreateScale(GlobalScale) * Matrix4.CreateTranslation(GlobalPosition);

            Mesh.Update(CubeGameWindow.Instance.CurrentGameScene.ActiveCamera, CubeGameWindow.Instance.WindowWidth, CubeGameWindow.Instance.Windowheight);

            base.OnUpdate();
        }

        public void OnRender()
        {
            Mesh.Render();
        }

        
    }
}
