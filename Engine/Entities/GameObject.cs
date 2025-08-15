using CubeEngine.Engine.Client.Graphics.Window;
using OpenTK.Mathematics;

namespace CubeEngine.Engine.Entities
{
    public class GameObject
    {
        protected Vector3 front { get; set; } = -Vector3.UnitZ;
        protected Vector3 up { get; set; } = Vector3.UnitY;
        protected Vector3 right { get; set; } = Vector3.UnitX;

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
                    _orientation = Quaternion.Identity; // default orientation
                }
                else
                {
                    _orientation = value;
                }
            }
        }

        public Vector3 Scale { get; set; }


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
