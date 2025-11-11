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

        public PlayerCharacter(Vector3 position)
        {
            Position = position;

            _movement = new MovementController(
                moveSpeed: 2f,
                gravity: -9.81f,
                jumpForce: 5f
            );

            GameClient.Instance.ServerMessage += OnServerMessage;

            List<Vector2> chunksToGen =
            [
                new(0, 0), new(1, 0), new(2, 0), new(3, 0), new(4, 0),
                new(0, 1), new(1, 1), new(2, 1), new(3, 1), new(4, 1),
                new(0, 2), new(1, 2), new(2, 2), new(3, 2), new(4, 2),
                new(0, 3), new(1, 3), new(2, 3), new(3, 3), new(4, 3),
                new(0, 4), new(1, 4), new(2, 4), new(3, 4), new(4, 4),
            ];

            foreach (var chunk in chunksToGen)
            {
                ChunkRequestPacket chunkRequestPacket = new(chunk);
                _ = GameClient.Instance.SendTcpMessage(chunkRequestPacket);
            }
        }

        private void OnServerMessage(Packet packet)
        {
            switch (packet)
            {
                case PlayerStatePacket playerStatePacket:
                    Position = playerStatePacket.Position;
                    Orientation = playerStatePacket.Orientation;
                    break;
                default:
                    break;
            }
        }

        public override void OnUpdate()
        {
            if (!CubeGameWindow.Instance.IsFocused)
                return;

            HandleInput();

            Vector3 postion = Position;
            _movement.ApplyGravity(ref postion, (float)Time.DeltaTime, GetGroundHeight);
            Position = postion;
        }

        private void HandleInput()
        {
            KeyboardState input = CubeGameWindow.Instance.KeyboardState;
            Vector3 moveDir = Vector3.Zero;

            // Movement
            if (input.IsKeyDown(Keys.W)) moveDir += FlatFront;
            if (input.IsKeyDown(Keys.S)) moveDir -= FlatFront;
            if (input.IsKeyDown(Keys.A)) moveDir -= FlatRight;
            if (input.IsKeyDown(Keys.D)) moveDir += FlatRight;

            Vector3 position = Position;
            _movement.HandleMovement(
                ref position,
                moveDir,
                (float)Time.DeltaTime,
                GetGroundHeight
            );
            Position = position;

            if (input.IsKeyDown(Keys.Space))
                _movement.Jump();

            List<PlayerInput> inputs = new();

            if (input.IsKeyDown(Keys.W)) inputs.Add(PlayerInput.MoveForward);
            if (input.IsKeyDown(Keys.S)) inputs.Add(PlayerInput.MoveBackward);
            if (input.IsKeyDown(Keys.A)) inputs.Add(PlayerInput.MoveLeft);
            if (input.IsKeyDown(Keys.D)) inputs.Add(PlayerInput.MoveRight);
            if (input.IsKeyDown(Keys.Space)) inputs.Add(PlayerInput.Jump);

            PlayerInputPacket playerInputPacket = new(
                (ushort)GameClient.Instance.ClientId,
                inputs
            );

            _ = GameClient.Instance.SendTcpMessage(playerInputPacket);
        }

        private float GetGroundHeight(Vector3 position)
        {
            var map = CubeGameWindow.Instance.CurrentGameScene.Map;
            if (map == null || map.CurrentChunks.Count == 0)
                return 0f;

            int wx = (int)MathF.Floor(position.X);
            int wz = (int)MathF.Floor(position.Z);

            float highestSolid = 0f;

            foreach (var chunk in map.CurrentChunks)
            {
                var data = chunk.ChunkData;
                Vector2 chunkOrigin = data.Position;

                int cx = (int)(wx - chunkOrigin.X);
                int cz = (int)(wz - chunkOrigin.Y);

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

            return highestSolid;
        }

        private Vector3 FlatFront
        {
            get
            {
                var f = GlobalFront;
                f.Y = 0;   
                return Vector3.Normalize(f);
            }
        }

        private Vector3 FlatRight
        {
            get
            {
                var r = GlobalRight;
                r.Y = 0;
                return Vector3.Normalize(r);
            }
        }
    }
}
