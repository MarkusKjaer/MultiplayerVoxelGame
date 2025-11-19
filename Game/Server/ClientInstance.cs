using CubeEngine.Engine.Entities;
using CubeEngine.Engine.Network;
using OpenTK.Mathematics;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace CubeEngine.Engine.Server
{
    public class ClientInstance
    {
        private Vector3 _postion;
        public Vector3 Position => _postion;

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
                moveSpeed: 2f,
                gravity: -9.81f,
                jumpForce: 5f
            );

            GameServer.Instance.ClientMessage += OnClientMessage;
        }

        public void Setup(Vector3 startPostion)
        {
            _postion = startPostion;
        }

        private void OnClientMessage(IPEndPoint client, Packet packet)
        {
            if (client.Address != EndPoint.Address)
                return;

            switch (packet)
            {
                case PlayerInputPacket inp:
                    DoPlayerInput(inp.Inputs);
                    break;
                case PlayerRotationPacket rotationPacket:
                    Rotate(rotationPacket.RotateX, 0);
                    Head.Rotate(0, rotationPacket.RotateY);
                    break;
            }
        }

        public void Update()
        {
            _movement.ApplyGravity(
                ref _postion,
                GameServer.ServerDeltaTime,
                GetGroundHeight
            );
        }

        private void DoPlayerInput(List<PlayerInput> inputs)
        {
            Vector3 moveDir = Vector3.Zero;

            foreach (var input in inputs)
            {
                switch (input)
                {
                    case PlayerInput.MoveForward: moveDir += FlatFront; break;
                    case PlayerInput.MoveBackward: moveDir -= FlatFront; break;
                    case PlayerInput.MoveLeft: moveDir -= FlatRight; break;
                    case PlayerInput.MoveRight: moveDir += FlatRight; break;
                    case PlayerInput.Jump: _movement.Jump(); break;
                }
            }

            _movement.HandleMovement(
                ref _postion,
                moveDir,
                GameServer.ServerDeltaTime,
                GetGroundHeight
            );

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

        private float GetGroundHeight(Vector3 position)
        {
            var map = GameServer.Instance.ServerMap;
            if (map == null || map.CurrentChunks.Count == 0)
                return 0f;

            int wx = (int)MathF.Floor(position.X);
            int wz = (int)MathF.Floor(position.Z);

            float highestSolid = 0f;

            foreach (var chunk in map.CurrentChunks)
            {
                var data = chunk.Value.ChunkData;
                Vector2 chunkOrigin = data.Position;

                const int CHUNK_SIZE = 32;

                int chunkX = (int)chunkOrigin.X / CHUNK_SIZE;
                int chunkZ = (int)chunkOrigin.Y / CHUNK_SIZE;

                int chunkWorldX = chunkX * CHUNK_SIZE;
                int chunkWorldZ = chunkZ * CHUNK_SIZE;

                int cx = wx - chunkWorldX;
                int cz = wz - chunkWorldZ;


                if (cx < 0 || cz < 0 ||
                    cx >= data.Voxels.GetLength(0) ||
                    cz >= data.Voxels.GetLength(2))
                    continue;

                for (int y = data.Voxels.GetLength(1) - 1; y >= 0; y--)
                {
                    if (data.Voxels[cx, y, cz].VoxelType != Client.World.Enum.VoxelType.Empty)
                    {
                        float worldY = y + 1;
                        if (worldY > highestSolid)
                            highestSolid = worldY;

                        break;
                    }
                }
            }

            if (highestSolid < 20)
                Debug.WriteLine("ge");

            return highestSolid;
        }

        private void Rotate(float yawDegrees, float pitchDegrees)
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
