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
        private VisualGameObject _body;

        public void Setup(int clientId, Mesh bodyMesh, Mesh headMesh)
        {
            ClientId = clientId;

            GameClient.Instance.ServerMessage += OnServerMessage;

            _body = new()
            {
                Scale = new(0.5f, 0.5f, 0.5f),
            };

            _body.Mesh = bodyMesh;

            _head = new()
            {
                Position = new(0, 3.2f, 0),
                Parent = _body,
            };

            _head.Mesh = headMesh;

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
            Quaternion headRotation = packet.HeadOrientation;

            headRotation *= Quaternion.FromAxisAngle(Vector3.UnitY, MathHelper.Pi);

            headRotation *= Quaternion.FromEulerAngles(0, MathHelper.DegreesToRadians(-90), 0);

            _head.Orientation = headRotation;

            _body.Position = packet.Position;
            _body.Orientation = packet.Orientation;
        }
    }
}
