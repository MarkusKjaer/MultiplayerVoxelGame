using CubeEngine.Engine.Client.Graphics;
using CubeEngine.Engine.Entities;
using CubeEngine.Engine.Network;
using CubeEngine.Engine.Client.Graphics.MeshObject;
using CubeEngine.Engine.Client.Graphics.Window.Setup.Texture;
using MultiplayerVoxelGame.Game.Resources;

namespace CubeEngine.Engine.Client.PlayerRender
{
    public class PlayerRenderManager : GameObject
    {
        private List<PlayerRenderInstance> _instances = [];

        private static readonly string HeadObjFilePath = AssetsManager.Instance.LoadedAssets[("Head", AssetType.OBJ)].FilePath;
        private static readonly string HeadTexturePath = AssetsManager.Instance.LoadedAssets[("HeadColor", AssetType.PNG)].FilePath;
        private static readonly string BodyObjFilePath = AssetsManager.Instance.LoadedAssets[("Body", AssetType.OBJ)].FilePath;
        private static readonly string BodyTexturePath = AssetsManager.Instance.LoadedAssets[("BodyColor", AssetType.PNG)].FilePath;
        private static readonly string VertexShaderPath = AssetsManager.Instance.LoadedAssets[("GameObjectShader", AssetType.VERT)].FilePath;
        private static readonly string FragmentShaderPath = AssetsManager.Instance.LoadedAssets[("GameObjectShader", AssetType.FRAG)].FilePath;

        private OBJFileReader _objFileReader;
        private MeshInfo _headMeshInfo;
        private Material _headMaterial;
        private MeshInfo _bodyMeshInfo;
        private Material _bodyMaterial;
        private Mesh _headMesh;
        private Mesh _bodyMesh;

        public void Setup()
        {
            GameClient.Instance.ServerMessage += OnServerMessage;

            _objFileReader = new OBJFileReader();
            _headMeshInfo = _objFileReader.ReadOBJFile(HeadObjFilePath);
            TextureManager headTextureManager = new(HeadTexturePath);
            _headMaterial = new(VertexShaderPath, FragmentShaderPath, headTextureManager);

            _bodyMeshInfo = _objFileReader.ReadOBJFile(BodyObjFilePath);
            TextureManager bodyTextureManager = new(BodyTexturePath);
            _bodyMaterial = new(VertexShaderPath, FragmentShaderPath, bodyTextureManager);

            _headMesh = new(_headMeshInfo, _headMaterial);
            _bodyMesh = new(_bodyMeshInfo, _bodyMaterial);
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
                            instance.Setup(playerStatePacket.ClientId, _bodyMesh, _headMesh);
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
