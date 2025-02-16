using CubeEngine.Engine.Window;
using OpenTK.Mathematics;

namespace CubeEngine.Engine.Entities
{
    public class GameObject
    {
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }
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
