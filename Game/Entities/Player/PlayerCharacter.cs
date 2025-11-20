using CubeEngine.Engine.Client;
using CubeEngine.Engine.Client.Graphics.Window;
using CubeEngine.Engine.Network;
using CubeEngine.Engine.Util;
using CubeEngine.Util;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace CubeEngine.Engine.Entities.Player
{
    public class PlayerCharacter : GameObject
    {
        private MovementController _movement;

        private readonly Vector3 PlayerSize = new(0.6f, 1.8f, 0.6f);

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

            for (int x = 0; x <= 4; x++)
                for (int z = 0; z <= 4; z++)
                    _ = GameClient.Instance.SendTcpMessage(new ChunkRequestPacket(new Vector2(x, z)));
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

            _movement.MoveWithCollision(
                ref pos,
                velocity,
                (float)Time.DeltaTime,
                IsSolid
            );

            Position = pos;
        }

        private void HandleInput()
        {
            KeyboardState input = CubeGameWindow.Instance.KeyboardState;
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

            _ = GameClient.Instance.SendTcpMessage(
                new PlayerInputPacket((ushort)GameClient.Instance.ClientId, inputs)
            );
        }

        private bool IsSolid(int x, int y, int z)
        {
            var map = CubeGameWindow.Instance.CurrentGameScene.Map;
            if (map == null) return false;

            foreach (var chunk in map.CurrentChunks)
            {
                var data = chunk.ChunkData;
                Vector2 origin = data.Position;
                int cx = x - (int)origin.X;
                int cz = z - (int)origin.Y;

                if (cx < 0 || cz < 0 ||
                    cx >= data.Voxels.GetLength(0) ||
                    cz >= data.Voxels.GetLength(2))
                    continue;

                if (y >= 0 && y < data.Voxels.GetLength(1))
                    return data.Voxels[cx, y, cz].VoxelType != Client.World.Enum.VoxelType.Empty;
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
