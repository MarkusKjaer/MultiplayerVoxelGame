using CubeEngine.Engine.Client.Graphics.Window;
using CubeEngine.Engine.Client.World.Enum;
using CubeEngine.Engine.Entities;
using CubeEngine.Engine.Network;
using CubeEngine.Engine.Util;
using MultiplayerVoxelGame.Util.Settings;
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

        private DateTime _lastBlockPlaced = DateTime.MinValue;
        private readonly TimeSpan _blockPlaceDelay = TimeSpan.FromMilliseconds(500);

        private DateTime _lastBlockBroken = DateTime.MinValue;
        private readonly TimeSpan _blockBreakDelay = TimeSpan.FromMilliseconds(500);

        public readonly SemaphoreSlim TcpSendLock = new SemaphoreSlim(1, 1);

        public IPEndPoint? UdpEndPoint { get; set; } = null;

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
                moveSpeed: PlayerSettings.moveSpeed,
                gravity: PlayerSettings.gravity,
                jumpForce: PlayerSettings.jumpForce,
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
                    case PlayerInput.BreakBlock: TryBreakBlock(); break;
                    case PlayerInput.PlaceBlock: TryPlaceBlock(); break;
                }
            }

            _moveDir = _movement.HandleMovement(_moveDir);
        }

        private bool IsSolid(int x, int y, int z)
        {
            var map = GameServer.Instance.ServerMap;
            if (map == null) return false;

            int chunkX = (int)MathF.Floor(x / (float)ChunkSettings.Width);
            int chunkZ = (int)MathF.Floor(z / (float)ChunkSettings.Width);

            if (map.CurrentChunks.TryGetValue(new Vector2(chunkX, chunkZ), out var chunk))
            {
                var data = chunk.ServerChunk;

                int lx = x - chunkX * ChunkSettings.Width;
                int lz = z - chunkZ * ChunkSettings.Width;

                if (y >= 0 && y < data.ChunkData.SizeY)
                {
                    var isNotSolid = data.GetVoxel(lx, y, lz) == Client.World.Enum.VoxelType.Empty;
                    isNotSolid |= data.GetVoxel(lx, y, lz) == Client.World.Enum.VoxelType.Water;
                    return !isNotSolid;
                }
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

        private bool RaycastBlock(out Vector3i hit, out Vector3i place)
        {
            Vector3 origin = _position + new Vector3(0, 1.6f, 0);

            Quaternion combinedOrientation = Orientation * Head.Orientation;

            Vector3 dir = Vector3.Transform(-Vector3.UnitZ, combinedOrientation);
            const float maxDist = 5f;

            float step = 0.05f;
            float dist = 0f;

            while (dist < maxDist)
            {
                Vector3 sample = origin + dir * dist;
                int x = (int)Math.Floor(sample.X);
                int y = (int)Math.Floor(sample.Y);
                int z = (int)Math.Floor(sample.Z);

                if (IsSolid(x, y, z))
                {
                    hit = new Vector3i(x, y, z);

                    Vector3 back = sample - dir * step;
                    place = new Vector3i(
                        (int)Math.Floor(back.X),
                        (int)Math.Floor(back.Y),
                        (int)Math.Floor(back.Z)
                    );
                    return true;
                }

                dist += step;
            }

            hit = default;
            place = default;
            return false;
        }

        private void TryBreakBlock()
        {
            if ((DateTime.Now - _lastBlockBroken) < _blockBreakDelay)
                return;

            if (!RaycastBlock(out Vector3i hit, out _)) return;

            var map = GameServer.Instance.ServerMap;
            if (map == null) return;

            map.SetBlock(hit.X, hit.Y, hit.Z, VoxelType.Empty);
            map.BroadcastChunkUpdate(hit);

            _lastBlockBroken = DateTime.Now;
        }


        private void TryPlaceBlock()
        {
            if ((DateTime.Now - _lastBlockPlaced) < _blockPlaceDelay)
                return;

            if (!RaycastBlock(out _, out Vector3i place)) return;

            BoundingBox blockBounds = new BoundingBox(
                new Vector3(place.X, place.Y, place.Z),
                new Vector3(place.X + 1, place.Y + 1, place.Z + 1)
            );

            if (BoundingBox.Intersects(blockBounds))
                return; 

            var map = GameServer.Instance.ServerMap;
            if (map == null) return;

            map.SetBlock(place.X, place.Y, place.Z, VoxelType.Dirt);
            map.BroadcastChunkUpdate(place);

            _lastBlockPlaced = DateTime.Now;
        }




        private Vector3 Right => Vector3.Transform(Vector3.UnitX, Orientation);
    }
}
