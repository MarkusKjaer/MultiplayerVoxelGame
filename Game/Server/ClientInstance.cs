using CubeEngine.Engine.Entities;
using CubeEngine.Engine.Network;
using CubeEngine.Engine.Util;
using OpenTK.Mathematics;
using System.Net;
using System.Net.Sockets;

namespace CubeEngine.Engine.Server
{
    public class ClientInstance
    {
        private Vector3 _position;
        public Vector3 Position => _position;

        private const float PLAYER_WIDTH = 0.6f;
        private const float PLAYER_HEIGHT = 1.8f;

        Vector3 _moveDir = Vector3.Zero;

        public BoundingBox BoundingBox
        {
            get
            {
                Vector3 half = new Vector3(PLAYER_WIDTH / 2f, 0, PLAYER_WIDTH / 2f);
                return new BoundingBox(
                    _position - half,
                    _position + new Vector3(half.X, PLAYER_HEIGHT, half.Z)
                );
            }
        }

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

        public ClientHead Head { get; private set; }
        private MovementController _movement;

        private static int _nextClientId = 0;
        public ushort Id { get; }
        public TcpClient TcpClient { get; }
        public IPEndPoint EndPoint { get; }
        public DateTime ConnectedAt { get; } = DateTime.Now;

        public string Username { get; set; } = "Unknown";

        public ClientInstance(TcpClient client)
        {
            Id = (ushort)Interlocked.Increment(ref _nextClientId);

            Head = new();
            TcpClient = client;
            EndPoint = (IPEndPoint)client.Client.RemoteEndPoint!;

            _movement = new MovementController(
                moveSpeed: 3f,
                gravity: -9.81f,
                jumpForce: 6f,
                playerSize: new Vector3(PLAYER_WIDTH, PLAYER_HEIGHT, PLAYER_WIDTH)
            );

            GameServer.Instance.ClientMessage += OnClientMessage;
        }

        public void Setup(Vector3 startPosition)
        {
            _position = startPosition;
        }

        private void OnClientMessage(IPEndPoint client, Packet packet)
        {
            if (client.Address != EndPoint.Address) return;

            switch (packet)
            {
                case PlayerInputPacket inp:
                    HandlePlayerInput(inp.Inputs);
                    break;

                case PlayerRotationPacket rotationPacket:
                    Rotate(rotationPacket.RotateX, 0);
                    Head.Rotate(0, rotationPacket.RotateY);
                    break;
            }
        }

        public void Update()
        {
            _movement.PrePhysicsGroundCheck(ref _position, IsSolid);

            Vector3 velocity = _movement.ApplyGravity(GameServer.ServerDeltaTime) + _moveDir;

            _movement.MoveWithCollision(
                ref _position,
                velocity,
                GameServer.ServerDeltaTime,
                IsSolid
            );
        }


        private void HandlePlayerInput(List<PlayerInput> inputs)
        {
            _moveDir = Vector3.Zero;

            foreach (var input in inputs)
            {
                switch (input)
                {
                    case PlayerInput.MoveForward: _moveDir += FlatFront; break;
                    case PlayerInput.MoveBackward: _moveDir -= FlatFront; break;
                    case PlayerInput.MoveLeft: _moveDir -= FlatRight; break;
                    case PlayerInput.MoveRight: _moveDir += FlatRight; break;
                    case PlayerInput.Jump: _movement.Jump(ref _position); break;
                }
            }

            _moveDir = _movement.HandleMovement(_moveDir);
        }

        private bool IsSolid(int x, int y, int z)
        {
            var map = GameServer.Instance.ServerMap;
            if (map == null) return false;

            foreach (var chunk in map.CurrentChunks)
            {
                var data = chunk.Value.ChunkData;
                int sx = data.Voxels.GetLength(0);
                int sy = data.Voxels.GetLength(1);
                int sz = data.Voxels.GetLength(2);

                Vector2 origin = data.Position;
                int cx = x - (int)origin.X;
                int cz = z - (int)origin.Y;

                if (cx < 0 || cz < 0 ||
                    cx >= sx || cz >= sz) continue;

                if (y >= 0 && y < sy)
                    return data.Voxels[cx, y, cz].VoxelType != Client.World.Enum.VoxelType.Empty;
            }

            return false;
        }

        private Vector3 FlatFront
        {
            get
            {
                var f = Vector3.Transform(-Vector3.UnitZ, Orientation);
                f.Y = 0;
                return f.LengthSquared > 0 ? Vector3.Normalize(f) : Vector3.UnitZ;
            }
        }

        private Vector3 FlatRight
        {
            get
            {
                var r = Vector3.Transform(Vector3.UnitX, Orientation);
                r.Y = 0;
                return r.LengthSquared > 0 ? Vector3.Normalize(r) : Vector3.UnitX;
            }
        }

        private void Rotate(float yawDegrees, float pitchDegrees)
        {
            float yaw = MathHelper.DegreesToRadians(yawDegrees);
            float pitch = MathHelper.DegreesToRadians(pitchDegrees);

            var yawRot = Quaternion.FromAxisAngle(Vector3.UnitY, yaw);
            var pitchRot = Quaternion.FromAxisAngle(Right, pitch);

            Orientation = yawRot * pitchRot * Orientation;
        }

        private Vector3 Right => Vector3.Transform(Vector3.UnitX, Orientation);
    }
}
