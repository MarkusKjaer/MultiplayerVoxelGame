using CubeEngine.Engine.Client.Graphics.MeshObject;
using CubeEngine.Engine.Client.Graphics.Window.Setup.Texture;
using CubeEngine.Engine.Entities;
using CubeEngine.Engine.Network;
using OpenTK.Mathematics;

namespace CubeEngine.Engine.Client.PlayerRender
{
    public class PlayerRenderInstance
    {
        public int ClientId { get; private set; }

        private VisualGameObject _head;
        private GameObject _body;

        public void Setup(int clientId, Mesh mesh)
        {
            ClientId = clientId;

            GameClient.Instance.ServerMessage += OnServerMessage;

            _body = new();
            _head = new()
            {
                Position = new(0, 2, 0),
                Parent = _body,
            };

            _head.Mesh = mesh;

            _body.Instantiate();
            _head.Instantiate();
        }


        private void OnServerMessage(Packet packet)
        {
            switch (packet)
            {
                case PlayerStatePacket playerStatePacket:
                    if(playerStatePacket.ClientId == ClientId)
                        HandlePlayerStatePacket(playerStatePacket);
                    break;
                default:
                    break;
            }
        }

        private void HandlePlayerStatePacket(PlayerStatePacket packet) 
        {
            _head.Orientation = packet.HeadOrientation * Quaternion.FromAxisAngle(Vector3.UnitY, MathHelper.Pi);
            _body.Position = packet.Position;
            _body.Orientation = packet.Orientation;
        }
    }
}
