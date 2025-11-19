using OpenTK.Mathematics;

namespace CubeEngine.Engine.Server
{
    public class ClientHead
    {
        private Quaternion _orientation = Quaternion.Identity;
        public Quaternion Orientation
        {
            get => _orientation;
            set
            {
                if (float.IsNaN(value.X) || float.IsNaN(value.Y) ||
                    float.IsNaN(value.Z) || float.IsNaN(value.W))
                {
                    _orientation = Quaternion.Identity;
                }
                else
                {
                    _orientation = value;
                }
            }
        }

        public void Rotate(float yawDegrees, float pitchDegrees)
        {
            float yaw = MathHelper.DegreesToRadians(yawDegrees);
            float pitch = MathHelper.DegreesToRadians(pitchDegrees);

            var yawRotation = Quaternion.FromAxisAngle(Vector3.UnitY, yaw);
            var pitchRotation = Quaternion.FromAxisAngle(Right, pitch);

            Orientation = yawRotation * pitchRotation * Orientation;
        }

        private Vector3 Right => Vector3.Transform(Vector3.UnitX, Orientation);
    }
}
