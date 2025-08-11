
using CubeEngine.Engine.Entities;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using System.Collections.ObjectModel;

namespace CubeEngine.Engine
{
    public class GameScene
    {
        public string Name { get; private set; }
        public Camera ActiveCamera { get; set; }

        private List<GameObject> _gameObjects;
        public ReadOnlyCollection<GameObject> GameObjects => _gameObjects.AsReadOnly();

        public GameScene(string name) 
        {
            this.Name = name;

            ActiveCamera = new(Vector3.Zero);
            _gameObjects = [];
        }

        public void AddGameObject(GameObject gameObject)
        {
            _gameObjects.Add(gameObject);

            gameObject.OnLoad();
        }

        public void RemoveGameObject(GameObject gameObject) 
        {
            bool isRemoved = _gameObjects.Remove(gameObject);

            if (isRemoved) 
            {
                gameObject.OnUnload();
            }
        }

        public virtual void Update()
        {
            for (int i = 0; i < _gameObjects.Count; i++)
            {
                _gameObjects[i].OnUpdate();
            }
        }

        public virtual void Render()
        {
            for (int i = 0; i < _gameObjects.Count; i++)
            {
                if (_gameObjects[i] is VisualGameObject visualGameObject) 
                {
                    visualGameObject.OnRender();
                }
            }
        }
    }
}
