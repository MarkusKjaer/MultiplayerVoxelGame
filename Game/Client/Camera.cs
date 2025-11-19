using CubeEngine.Engine.Entities;
using OpenTK.Mathematics;

namespace CubeEngine.Engine.Client
{
    public class Camera : GameObject
    {
        public Camera(Vector3 position)
        {
            Position = position;

            Orientation = Quaternion.Identity; // Default orientation
        }

        public void LookAt(Vector3 target)
        {
            Matrix4 lookRotation = Matrix4.LookAt(GlobalPosition, target, GlobalUp);
            Orientation = lookRotation.ExtractRotation();
        }

        public Matrix4 GetCurrentView()
        {
            return Matrix4.LookAt(GlobalPosition, GlobalPosition + GlobalFront, GlobalUp);
        }
    }
}
