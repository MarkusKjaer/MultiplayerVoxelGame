using CubeEngine.Engine.Entities;
using OpenTK.Mathematics;

namespace CubeEngine.Engine
{
    public class Camera : GameObject
    {
        public readonly Vector3 Up = Vector3.UnitY;
        public readonly Vector3 Front = -Vector3.UnitZ;

        public Vector3 Target;

        public Camera(Vector3 position)
        {
            Position = position;
        }

        public Matrix4 LookAt(Vector3 target)
        {
            Target = target;
            return Matrix4.LookAt(Position, target, Up);
        }

        public Matrix4 GetCurrentView()
        {
            return Matrix4.LookAt(Position, Target, Up);
        }
    }
}
