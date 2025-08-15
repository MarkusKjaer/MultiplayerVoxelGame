using CubeEngine.Engine.Entities;
using OpenTK.Mathematics;

namespace CubeEngine.Engine.Client
{
    public class Camera : GameObject
    {
        float _yaw { get; set; } = -90.0f; // Yaw is initialized to -90 degrees to face the camera forward
        float _pitch { get; set; } = 0.0f; // Pitch is initialized to 0 degrees (looking straight ahead)

        public Camera(Vector3 position)
        {
            Position = position;

            Orientation = Quaternion.Identity; // Default orientation
        }

        public void Rotate(float xoffset, float yoffset)
        {
            _yaw += xoffset;
            _pitch += yoffset;

            if (_pitch > 89.0f)
                _pitch = 89.0f;
            if (_pitch < -89.0f)
                _pitch = -89.0f;

            Vector3 direction;
            direction.X = (float)(Math.Cos(MathHelper.DegreesToRadians(_yaw)) * Math.Cos(MathHelper.DegreesToRadians(_pitch)));
            direction.Y = (float)Math.Sin(MathHelper.DegreesToRadians(_pitch));
            direction.Z = (float)(Math.Sin(MathHelper.DegreesToRadians(_yaw)) * Math.Cos(MathHelper.DegreesToRadians(_pitch)));

            front = Vector3.Normalize(direction);
            right = Vector3.Normalize(Vector3.Cross(front, up)); // Recalculate right vector

            Orientation = Quaternion.FromEulerAngles(
                MathHelper.DegreesToRadians(_pitch),
                MathHelper.DegreesToRadians(_yaw),
                0.0f
            );
        }

        public void LookAt(Vector3 target)
        {
            Matrix4 lookRotation = Matrix4.LookAt(Position, target, up);
            Orientation = lookRotation.ExtractRotation();
        }

        public Matrix4 GetCurrentView()
        {
            return Matrix4.LookAt(Position, Position + front, up);
        }
    }
}
