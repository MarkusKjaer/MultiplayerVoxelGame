using CubeEngine.Engine.MeshObject;
using CubeEngine.Engine.Window;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;

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
            // Convert degrees to radians
            float rotX = MathHelper.DegreesToRadians(Orientation.X);
            float rotY = MathHelper.DegreesToRadians(Orientation.Y);
            float rotZ = MathHelper.DegreesToRadians(Orientation.Z);

            // Create rotation matrices for each axis
            Matrix4 rotationX = Matrix4.CreateRotationX(rotX);
            Matrix4 rotationY = Matrix4.CreateRotationY(rotY);
            Matrix4 rotationZ = Matrix4.CreateRotationZ(rotZ);

            // Combine rotations: Usually Y * X * Z order is common for Euler angles
            Matrix4 rotationMatrix = rotationY * rotationX * rotationZ;

            // Create translation matrix
            Matrix4 translation = Matrix4.CreateTranslation(Position);

            Mesh.Model = rotationMatrix * translation;

            Mesh.Update(CubeGameWindow.Instance.CurrentGameScene.ActiveCamera, CubeGameWindow.Instance.WindowWidth, CubeGameWindow.Instance.Windowheight);
            base.OnUpdate();
        }

        public void OnRender()
        {
            Mesh.Render();
        }

        
    }
}
