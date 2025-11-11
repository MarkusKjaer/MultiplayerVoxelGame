using OpenTK.Mathematics;

namespace CubeEngine.Engine.Entities
{
    public class MovementController
    {
        private float _moveSpeed;
        private float _gravity;
        private float _jumpForce;
        private float _verticalVelocity = 0f;
        private bool _isGrounded = true;

        public MovementController(float moveSpeed = 0.05f, float gravity = -0.01f, float jumpForce = 0.25f)
        {
            _moveSpeed = moveSpeed;
            _gravity = gravity;
            _jumpForce = jumpForce;
        }

        public void HandleMovement(ref Vector3 position, Vector3 moveDir, float deltaTime, Func<Vector3, float> getGroundHeight)
        {
            if (moveDir != Vector3.Zero)
            {
                moveDir = Vector3.Normalize(moveDir);
                Move(ref position, moveDir, _moveSpeed * deltaTime, getGroundHeight);
            }
        }

        public void Jump()
        {
            if (_isGrounded)
            {
                _verticalVelocity = _jumpForce;
                _isGrounded = false;
            }
        }

        public void ApplyGravity(ref Vector3 position, float deltaTime, Func<Vector3, float> getGroundHeight)
        {
            float groundY = getGroundHeight(position);

            if (!_isGrounded)
                _verticalVelocity += _gravity * deltaTime;

            position += new Vector3(0, _verticalVelocity * deltaTime, 0);

            if (position.Y <= groundY)
            {
                position = new Vector3(position.X, groundY, position.Z);
                _verticalVelocity = 0f;
                _isGrounded = true;
            }
            else
            {
                _isGrounded = false;
            }
        }

        private void Move(ref Vector3 position, Vector3 direction, float amount, Func<Vector3, float> getGroundHeight)
        {
            Vector3 newPos = position + direction * amount;
            float currentHeight = position.Y;
            float targetHeight = getGroundHeight(newPos);
            float heightDiff = targetHeight - currentHeight;

            if (heightDiff > 0)
            {
                Vector3 slideX = new Vector3(newPos.X, position.Y, position.Z);
                float slideXHeight = getGroundHeight(slideX);
                bool canSlideX = slideXHeight - currentHeight <= 0;

                Vector3 slideZ = new Vector3(position.X, position.Y, newPos.Z);
                float slideZHeight = getGroundHeight(slideZ);
                bool canSlideZ = slideZHeight - currentHeight <= 0;

                if (canSlideX && !canSlideZ)
                    position = slideX;
                else if (!canSlideX && canSlideZ)
                    position = slideZ;
                else if (canSlideX && canSlideZ)
                    position = MathF.Abs(direction.X) > MathF.Abs(direction.Z) ? slideX : slideZ;

                return;
            }

            position = newPos;
        }
    }
}
