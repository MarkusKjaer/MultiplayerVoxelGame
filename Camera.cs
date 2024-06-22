using System;
using OpenTK.Mathematics;

namespace CubeEngine
{
    public class Camera
    {
        public Vector3 Postion;

        public readonly Vector3 Up = Vector3.UnitY;
        public readonly Vector3 Front = -Vector3.UnitZ;

        public Vector3 Target;

        public Camera(Vector3 postion)
        {
            Postion = postion;
        }

        public Matrix4 LookAt(Vector3 target)
        {
            Target = target;
            return Matrix4.LookAt(Postion, target, Up);
        }

        public Matrix4 GetCurrentView()
        {
            return Matrix4.LookAt(Postion, Target, Up);
        }
    }
}
