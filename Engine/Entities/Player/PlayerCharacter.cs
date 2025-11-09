using CubeEngine.Engine.Client.Graphics.Window;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace CubeEngine.Engine.Entities.Player
{
    public class PlayerCharacter : GameObject
    {
        private float _moveSpeed = 0.05f;

        private float _gravity = -0.01f;
        private float _jumpForce = 0.25f;
        private float _verticalVelocity = 0f;
        private bool _isGrounded = true;

        public PlayerCharacter(Vector3 position)
        {
            Position = position;
        }


        public override void OnUpdate()
        {
            if (!CubeGameWindow.Instance.IsFocused)
                return;

            UpdateMovement();
            ApplyGravity();
        }

        private void UpdateMovement()
        {
            KeyboardState input = CubeGameWindow.Instance.KeyboardState;
            Vector3 moveDir = Vector3.Zero;

            if (input.IsKeyDown(Keys.W)) moveDir += FlatFront;
            if (input.IsKeyDown(Keys.S)) moveDir -= FlatFront;
            if (input.IsKeyDown(Keys.A)) moveDir -= FlatRight;
            if (input.IsKeyDown(Keys.D)) moveDir += FlatRight;

            if (moveDir != Vector3.Zero)
            {
                moveDir = Vector3.Normalize(moveDir);
                Move(moveDir, _moveSpeed);
            }

            if (input.IsKeyDown(Keys.Space) && _isGrounded)
            {
                _verticalVelocity = _jumpForce;
                _isGrounded = false;
            }
        }

        private void ApplyGravity()
        {
            float groundY = GetGroundHeight(Position);

            if (!_isGrounded)
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
                    if (data.Voxels[cx, y, cz].VoxelType != CubeEngine.Engine.Client.World.Enum.VoxelType.Empty)
                    {
                        float worldY = chunkOrigin.Y + y + 1;
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

        public void Move(Vector3 direction, float amount)
        {
            Vector3 newPos = Position + direction * amount;

            float currentHeight = Position.Y;
            float targetHeight = GetGroundHeight(newPos);

            float heightDiff = targetHeight - currentHeight;

            if (heightDiff > 0)
            {
                Vector3 slideX = new Vector3(newPos.X, Position.Y, Position.Z);
                float slideXHeight = GetGroundHeight(slideX);
                float slideXDiff = slideXHeight - currentHeight;

                bool canSlideX = slideXDiff <= 0;

                Vector3 slideZ = new Vector3(Position.X, Position.Y, newPos.Z);
                float slideZHeight = GetGroundHeight(slideZ);
                float slideZDiff = slideZHeight - currentHeight;

                bool canSlideZ = slideZDiff <= 0;

                if (canSlideX && !canSlideZ)
                    Position = slideX;
                else if (!canSlideX && canSlideZ)
                    Position = slideZ;
                else if (canSlideX && canSlideZ)
                {
                    float xMove = MathF.Abs(direction.X);
                    float zMove = MathF.Abs(direction.Z);

                    Position = (xMove > zMove) ? slideX : slideZ;
                }

                return;
            }

            Position = newPos;
        }
    }
}
