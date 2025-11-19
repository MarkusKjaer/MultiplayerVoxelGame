using CubeEngine.Engine.Client.Graphics;
using CubeEngine.Engine.Entities;
using CubeEngine.Engine.Network;
using CubeEngine.Engine.Client.Graphics.MeshObject;
using CubeEngine.Engine.Client.Graphics.Window.Setup.Texture;

namespace CubeEngine.Engine.Client.PlayerRender
{
    public class PlayerRenderManager : GameObject
    {
        private List<PlayerRenderInstance> _instances = [];

        private static readonly string BaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        private static readonly string ParentDirectory = Directory.GetParent(BaseDirectory).Parent.Parent.Parent.FullName;
        private static readonly string ObjFilePath = Path.Combine(ParentDirectory, "Models", "Suzanne.obj");
        private static readonly string TexturePath = Path.Combine(ParentDirectory, "Models", "ondskab.png");
        private static readonly string VertexShaderPath = Path.Combine(ParentDirectory, "Game", "Client", "Graphics", "Window", "Shaders", "Cube.vert");
        private static readonly string FragmentShaderPath = Path.Combine(ParentDirectory, "Game", "Client", "Graphics", "Window", "Shaders", "Cube.frag");

        private OBJFileReader _objFileReader;
        private MeshInfo _meshInfo;
        private Material _material;
        private Mesh _playerMesh;

        public void Setup()
        {
            GameClient.Instance.ServerMessage += OnServerMessage;

            _objFileReader = new OBJFileReader();
            _meshInfo = _objFileReader.ReadOBJFile(ObjFilePath);
            TextureManager textureManager = new(TexturePath);
            _material = new(VertexShaderPath, FragmentShaderPath, textureManager);

            _playerMesh = new(_meshInfo, _material);
        }

        private void OnServerMessage(Packet packet)
        {
            switch (packet)
            {
                case PlayerStatePacket playerStatePacket:

                    if (GameClient.Instance.ClientId == playerStatePacket.ClientId)
                        return;

                    var playerRender = _instances.Find(instance => instance.ClientId == playerStatePacket.ClientId);

                    if (playerRender == null)
                    {
                        GLActionQueue.Enqueue(() =>
                        {
                            PlayerRenderInstance instance = new();
                            instance.Setup(playerStatePacket.ClientId, _playerMesh);
                            _instances.Add(instance);
                        });
                    }

                    break;
                default:
                    break;
            }
        }
    }
}
