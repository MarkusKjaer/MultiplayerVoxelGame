using CubeEngine.Engine.Client.Graphics.Window;
using OpenTK.Mathematics;

namespace CubeEngine.Engine.Entities
{
    public class GameObject
    {
        protected Vector3 Front => Vector3.Transform(-Vector3.UnitZ, Orientation);
        protected Vector3 Up => Vector3.Transform(Vector3.UnitY, Orientation);
        protected Vector3 Right => Vector3.Transform(Vector3.UnitX, Orientation);

        public Vector3 Position { get; set; }
       

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

        public Vector3 Scale { get; set; } = new(1, 1, 1);

        #region Global

        public Vector3 GlobalFront
        {
            get
            {
                if (Parent is not null)
                    return Vector3.Transform(Front, Parent.GlobalOrientation);
                return Front;
            }
        }

        public Vector3 GlobalUp
        {
            get
            {
                if (Parent is not null)
                    return Vector3.Transform(Up, Parent.GlobalOrientation);
                return Up;
            }
        }

        public Vector3 GlobalRight
        {
            get
            {
                if (Parent is not null)
                    return Vector3.Transform(Right, Parent.GlobalOrientation);
                return Right;
            }
        }

        public Vector3 GlobalPosition
        {
            get
            {
                if (Parent is not null)
                {
                    return Parent.GlobalPosition
                        + Vector3.Transform(Position * Parent.GlobalScale, Parent.GlobalOrientation);
                }
                return Position;
            }
        }
        public Quaternion GlobalOrientation
        {
            get
            {
                if (Parent is not null)
                {
                    return Parent.GlobalOrientation * Orientation;
                }
                return Orientation;
            }
        }
        public Vector3 GlobalScale
        {
            get
            {
                if (Parent is not null)
                {
                    return Parent.GlobalScale * Scale;
                }
                return Scale;
            }
        }
        #endregion

        private GameObject? _parent;
        public GameObject? Parent
        {
            get => _parent;
            set
            {
                if (value != null && value == this)
                {
                    throw new InvalidOperationException("A GameObject cannot be its own parent.");
                }
                _parent = value;
            }
        }

        public void Rotate(float yawDegrees, float pitchDegrees)
        {
            // Convert to radians
            float yaw = MathHelper.DegreesToRadians(yawDegrees);
            float pitch = MathHelper.DegreesToRadians(pitchDegrees);

            // Create quaternions for yaw and pitch
            var yawRotation = Quaternion.FromAxisAngle(Vector3.UnitY, yaw);
            var pitchRotation = Quaternion.FromAxisAngle(Right, pitch);

            // Apply rotations: yaw first, then pitch
            Orientation = yawRotation * pitchRotation * Orientation;
        }

        public virtual void OnLoad()
        {

        }

        public virtual void OnUpdate()
        {

        }

        public virtual void OnUnload()
        {

        }

        public void Instantiate()
        {
            CubeGameWindow.Instance.CurrentGameScene.AddGameObject(this);
        }
    }
}
