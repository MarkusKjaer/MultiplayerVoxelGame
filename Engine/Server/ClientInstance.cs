using CubeEngine.Engine.Client.Graphics.Window;
using CubeEngine.Engine.Client.World;
using CubeEngine.Engine.Network;
using OpenTK.Mathematics;
using System.Net;
using System.Net.Sockets;
using static System.Formats.Asn1.AsnWriter;

namespace CubeEngine.Engine.Server
{
    public class ClientInstance
    {
        private static int _nextClientId = 0;
        public ushort Id { get; }
        public TcpClient TcpClient { get; }
        public IPEndPoint EndPoint { get; }
        public DateTime ConnectedAt { get; } = DateTime.Now;

        public string Username { get; set; } = "Unknown"; 

        public Vector3 Position { get; set; }
        public Quaternion Orientation { get; set; }

        private float _moveSpeed = 0.05f;
        private float _gravity = -0.01f;
        private float _jumpForce = 0.25f;
        private float _verticalVelocity = 0f;
        private bool _isGrounded = true;

        public ClientInstance(TcpClient client)
        {
            Id = (ushort)Interlocked.Increment(ref _nextClientId);

            TcpClient = client;
            EndPoint = (IPEndPoint)client.Client.RemoteEndPoint!;

            Setup();
        }

        private void Setup()
        {
            GameServer.Instance.OnClientMessage += OnClientMessage;
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
            }
        }

        public override string ToString()
        {
            return $"{Id} | {EndPoint}";
        }

        private void DoPlayerInput(List<PlayerInput> playerInputs)
        {
            Vector3 moveDir = Vector3.Zero;

            // Build movement vector from inputs
            foreach (var input in playerInputs)
            {
                switch (input)
                {
                    case PlayerInput.MoveForward: moveDir += FlatFront; break;
                    case PlayerInput.MoveBackward: moveDir -= FlatFront; break;
                    case PlayerInput.MoveLeft: moveDir -= FlatRight; break;
                    case PlayerInput.MoveRight: moveDir += FlatRight; break;
                    case PlayerInput.Jump:
                        if (_isGrounded)
                        {
                            _verticalVelocity = _jumpForce;
                            _isGrounded = false;
                        }
                        break;
                }
            }

            if (moveDir != Vector3.Zero)
            {
                moveDir = Vector3.Normalize(moveDir);
                Move(moveDir, _moveSpeed * GameServer.ServerDeltaTime);
            }

            ApplyGravity();
        }

        private void ApplyGravity()
        {
            float groundY = GetGroundHeight(Position);

            if (!_isGrounded)
                _verticalVelocity += _gravity;

            _verticalVelocity += _gravity;
            Position += new Vector3(0, _verticalVelocity, 0);


            if (Position.Y <= groundY)
            {
                Position = new Vector3(Position.X, groundY, Position.Z);
                _verticalVelocity = 0f;
                _isGrounded = true;
            }
            else
            {
                _isGrounded = false;
            }
        }

        private void Move(Vector3 direction, float amount)
        {
            Vector3 newPos = Position + direction * amount;
            float currentHeight = Position.Y;
            float targetHeight = GetGroundHeight(newPos);
            float heightDiff = targetHeight - currentHeight;

            if (heightDiff > 0)
            {
                Vector3 slideX = new Vector3(newPos.X, Position.Y, Position.Z);
                Vector3 slideZ = new Vector3(Position.X, Position.Y, newPos.Z);

                bool canSlideX = GetGroundHeight(slideX) - currentHeight <= 0;
                bool canSlideZ = GetGroundHeight(slideZ) - currentHeight <= 0;

                if (canSlideX && !canSlideZ) Position = slideX;
                else if (!canSlideX && canSlideZ) Position = slideZ;
                else if (canSlideX && canSlideZ)
                    Position = MathF.Abs(direction.X) > MathF.Abs(direction.Z) ? slideX : slideZ;

                return;
            }

            Position = newPos;
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

                int cx = (int)(wx - chunkOrigin.X);
                int cz = (int)(wz - chunkOrigin.Y);

                if (cx < 0 || cz < 0 ||
                    cx >= data.Voxels.GetLength(0) ||
                    cz >= data.Voxels.GetLength(2))
                    continue;

                for (int y = data.Voxels.GetLength(1) - 1; y >= 0; y--)
                {
                    if (data.Voxels[cx, y, cz].VoxelType != CubeEngine.Engine.Client.World.Enum.VoxelType.Empty)
                    {
                        float worldY = y + 1;
                        if (worldY > highestSolid)
                            highestSolid = worldY;

                        break;
                    }
                }
            }

            return highestSolid;
        }
    }
}
