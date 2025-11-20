using OpenTK.Mathematics;

namespace CubeEngine.Engine.Entities
{
    public class MovementController
    {
        private float _moveSpeed;
        private float _gravity;
        private float _jumpForce;
        private Vector3 _playerSize;

        private float _verticalVelocity = 0f;
        private bool _isGrounded = false;

        private const float SkinWidth = 0.05f;

        public MovementController(float moveSpeed = 2f, float gravity = -9.81f, float jumpForce = 5f, Vector3? playerSize = null)
        {
            _moveSpeed = moveSpeed;
            _gravity = gravity;
            _jumpForce = jumpForce;
            _playerSize = playerSize ?? new Vector3(0.6f, 1.8f, 0.6f);
        }

        public void Jump(ref Vector3 pos)
        {
            if (_isGrounded)
            {
                pos.Y += 0.05f;
                _verticalVelocity = _jumpForce;
                _isGrounded = false;
            }
        }

        public Vector3 HandleMovement(Vector3 moveDir)
        {
            if (moveDir == Vector3.Zero)
                return Vector3.Zero;

            return Vector3.Normalize(moveDir) * _moveSpeed;
        }

        public Vector3 ApplyGravity(float deltaTime)
        {
            if (!_isGrounded)
                _verticalVelocity += _gravity * deltaTime;

            return new Vector3(0, _verticalVelocity, 0);
        }

        public void PrePhysicsGroundCheck(ref Vector3 pos, Func<int, int, int, bool> isSolid)
        {
            _isGrounded = false;

            Vector3 half = new Vector3(_playerSize.X / 2f - SkinWidth, 0, _playerSize.Z / 2f - SkinWidth);
            Vector3 checkMin = new(pos.X - half.X, pos.Y - SkinWidth, pos.Z - half.Z);
            Vector3 checkMax = new(pos.X + half.X, pos.Y - SkinWidth, pos.Z + half.Z);

            int minX = (int)MathF.Floor(checkMin.X);
            int minY = (int)MathF.Floor(checkMin.Y);
            int minZ = (int)MathF.Floor(checkMin.Z);

            int maxX = (int)MathF.Floor(checkMax.X);
            int maxY = (int)MathF.Floor(checkMax.Y);
            int maxZ = (int)MathF.Floor(checkMax.Z);

            for (int x = minX; x <= maxX; x++)
                for (int y = minY; y <= maxY; y++)
                    for (int z = minZ; z <= maxZ; z++)
                    {
                        if (isSolid(x, y, z))
                        {
                            _isGrounded = true;
                            _verticalVelocity = 0;
                            return;
                        }
                    }
        }

        public void MoveWithCollision(ref Vector3 pos, Vector3 velocity, float dt, Func<int, int, int, bool> isSolid)
        {
            Vector3 move = velocity * dt;

            MoveAxis(ref pos, new Vector3(move.X, 0, 0), 0, isSolid);
            MoveAxis(ref pos, new Vector3(0, move.Y, 0), 1, isSolid);
            MoveAxis(ref pos, new Vector3(0, 0, move.Z), 2, isSolid);

        }

        private void MoveAxis(ref Vector3 pos, Vector3 delta, int axis, Func<int, int, int, bool> isSolid)
        {
            float move = delta[axis];
            if (move == 0)
                return;

            Vector3 half = new Vector3(
                _playerSize.X / 2f - SkinWidth,
                _playerSize.Y - SkinWidth,
                _playerSize.Z / 2f - SkinWidth
            );

            float sign = MathF.Sign(move);
            float remaining = MathF.Abs(move);

            while (remaining > 0f)
            {
                float step = MathF.Min(remaining, SkinWidth);
                pos[axis] += step * sign;

                Vector3 min = new(pos.X - half.X, pos.Y, pos.Z - half.Z);
                Vector3 max = new(pos.X + half.X, pos.Y + _playerSize.Y, pos.Z + half.Z);

                int minX = (int)MathF.Floor(min.X);
                int minY = (int)MathF.Floor(min.Y);
                int minZ = (int)MathF.Floor(min.Z);

                int maxX = (int)MathF.Floor(max.X);
                int maxY = (int)MathF.Floor(max.Y);
                int maxZ = (int)MathF.Floor(max.Z);

                bool collided = false;

                for (int x = minX; x <= maxX; x++)
                    for (int y = minY; y <= maxY; y++)
                        for (int z = minZ; z <= maxZ; z++)
                        {
                            if (isSolid(x, y, z))
                            {
                                collided = true;
                                break;
                            }
                        }

                if (collided)
                {
                    pos[axis] -= step * sign;

                    if (axis == 1)
                    {
                        _verticalVelocity = 0;
                        if (sign < 0)
                            _isGrounded = true;
                    }

                    return;
                }

                remaining -= step;
            }
        }

    }
}
