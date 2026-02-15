using CubeEngine.Engine.Client;
using CubeEngine.Engine.Client.Graphics.Window;
using CubeEngine.Engine.Network;
using CubeEngine.Util;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace CubeEngine.Engine.Entities.Player
{
    public class PlayerCharacter : GameObject
    {
        private MovementController _movement;

        private readonly Vector3 PlayerSize = new(0.6f, 1.8f, 0.6f);

        private float _chunkCheckCooldown = 0f;
        private const float ChunkCheckInterval = 5f;
        private const int ChunkRadius = 1;

        public PlayerCharacter(Vector3 position)
        {
            Position = position;

            _movement = new MovementController(
                moveSpeed: 2f,
                gravity: -9.81f,
                jumpForce: 5f,
                playerSize: PlayerSize
            );

            GameClient.Instance.ServerMessage += OnServerMessage;
        }

        private void OnServerMessage(Packet packet)
        {
            if (packet is PlayerStatePacket playerStatePacket &&
                playerStatePacket.ClientId == GameClient.Instance.ClientId)
            {
                Position = playerStatePacket.Position;
                Orientation = playerStatePacket.Orientation;
            }
        }

        public override void OnUpdate()
        {
            if (!CubeGameWindow.Instance.IsFocused) return;

            HandleInput();

            Vector3 pos = Position;
            _movement.PrePhysicsGroundCheck(ref pos, IsSolid);
            Vector3 velocity = _movement.ApplyGravity((float)Time.DeltaTime);
            _movement.MoveWithCollision(ref pos, velocity, (float)Time.DeltaTime, IsSolid);
            Position = pos;

            _chunkCheckCooldown -= (float)Time.DeltaTime;
            if (_chunkCheckCooldown <= 0f)
            {
                RequestNearbyChunks();
                _chunkCheckCooldown = ChunkCheckInterval;
            }
        }


        private void HandleInput()
        {
            KeyboardState input = CubeGameWindow.Instance.KeyboardState;
            MouseState mouse = CubeGameWindow.Instance.MouseState;

            Vector3 moveDir = Vector3.Zero;

            if (input.IsKeyDown(Keys.W)) moveDir += FlatFront;
            if (input.IsKeyDown(Keys.S)) moveDir -= FlatFront;
            if (input.IsKeyDown(Keys.A)) moveDir -= FlatRight;
            if (input.IsKeyDown(Keys.D)) moveDir += FlatRight;

            Vector3 horizontalVelocity = _movement.HandleMovement(moveDir);
            Vector3 pos = Position;

            if (input.IsKeyDown(Keys.Space)) _movement.Jump(ref pos);

            _movement.MoveWithCollision(
                ref pos,
                horizontalVelocity,
                (float)Time.DeltaTime,
                IsSolid
            );

            Position = pos;

            List<PlayerInput> inputs = new();
            if (input.IsKeyDown(Keys.W)) inputs.Add(PlayerInput.MoveForward);
            if (input.IsKeyDown(Keys.S)) inputs.Add(PlayerInput.MoveBackward);
            if (input.IsKeyDown(Keys.A)) inputs.Add(PlayerInput.MoveLeft);
            if (input.IsKeyDown(Keys.D)) inputs.Add(PlayerInput.MoveRight);
            if (input.IsKeyDown(Keys.Space)) inputs.Add(PlayerInput.Jump);
            if (mouse.IsButtonDown(MouseButton.Left)) inputs.Add(PlayerInput.BreakBlock);
            if (mouse.IsButtonDown(MouseButton.Right)) inputs.Add(PlayerInput.PlaceBlock);

            _ = GameClient.Instance.SendTcpMessage(
                new PlayerInputPacket((ushort)GameClient.Instance.ClientId, inputs)
            );
        }

        private void RequestNearbyChunks()
        {
            var map = CubeGameWindow.Instance.CurrentGameScene.Map;
            if (map == null) return;

            int playerChunkX = (int)MathF.Floor(Position.X / 16);
            int playerChunkZ = (int)MathF.Floor(Position.Z / 16);

            for (int dx = -ChunkRadius; dx <= ChunkRadius; dx++)
            {
                for (int dz = -ChunkRadius; dz <= ChunkRadius; dz++)
                {
                    Vector2 chunkCoords = new(playerChunkX + dx, playerChunkZ + dz);
                    Vector2 worldPos = chunkCoords * 16;

                    if (!map.CurrentChunks.ContainsKey(worldPos))
                    {
                        _ = GameClient.Instance.SendTcpMessage(new ChunkRequestPacket(chunkCoords));
                    }
                }
            }
        }


        private bool IsSolid(int x, int y, int z)
        {
            var map = CubeGameWindow.Instance.CurrentGameScene.Map;
            if (map == null) return false;

            int chunkX = (int)MathF.Floor(x / 16f) * 16;
            int chunkZ = (int)MathF.Floor(z / 16f) * 16;
            Vector2 chunkKey = new(chunkX, chunkZ);

            if (map.CurrentChunks.TryGetValue(chunkKey, out var chunk))
            {
                var data = chunk.ChunkData;

                int lx = x - chunkX;
                int lz = z - chunkZ;

                if (y >= 0 && y < data.SizeY)
                {
                    return data.GetVoxel(lx, y, lz).VoxelType != Client.World.Enum.VoxelType.Empty;
                }
            }

            return false;
        }

        private Vector3 FlatFront
        {
            get
            {
                var f = GlobalFront;
                f.Y = 0;
                return f.LengthSquared > 0 ? Vector3.Normalize(f) : Vector3.UnitZ;
            }
        }

        private Vector3 FlatRight
        {
            get
            {
                var r = GlobalRight;
                r.Y = 0;
                return r.LengthSquared > 0 ? Vector3.Normalize(r) : Vector3.UnitX;
            }
        }
    }
}
