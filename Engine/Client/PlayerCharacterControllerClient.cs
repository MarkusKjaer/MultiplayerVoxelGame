using CubeEngine.Engine.Client.Graphics.Window;
using CubeEngine.Engine.Entities;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace CubeEngine.Engine.Client
{
    public class PlayerFlyControllerClient : GameObject
    {
        private float _moveSpeed = 0.2f;
        private float _mouseSensitivity = 0.025f;

        public PlayerFlyControllerClient() : base()
        {
            CubeGameWindow.Instance.CursorState = CursorState.Grabbed; // Grab the cursor to prevent it from leaving the window
        }

        public override void OnUpdate()
        {
            if (!CubeGameWindow.Instance.IsFocused)
                return;

            var mouseDelta = CubeGameWindow.Instance.MouseState.Delta;

            float xoffset = -mouseDelta.X * _mouseSensitivity;
            float yoffset = -mouseDelta.Y * _mouseSensitivity;

            float currentPitch = MathHelper.RadiansToDegrees(
                MathF.Asin(GlobalFront.Y) // extract pitch from forward vector
            );

            float newPitch = currentPitch + yoffset;
            if (newPitch > 89.0f || newPitch < -89.0f)
            {
                // Clamp pitch to prevent flipping
                yoffset = 0;
            }

            Rotate(xoffset, yoffset);

            // Movement
            KeyboardState input = CubeGameWindow.Instance.KeyboardState;
            Vector3 moveDir = Vector3.Zero;

            if (input.IsKeyDown(Keys.W)) moveDir += Front;
            if (input.IsKeyDown(Keys.S)) moveDir -= Front;
            if (input.IsKeyDown(Keys.A)) moveDir -= Right;
            if (input.IsKeyDown(Keys.D)) moveDir += Right;
            if (input.IsKeyDown(Keys.Space)) moveDir += new Vector3(0, 1, 0);
            if (input.IsKeyDown(Keys.LeftShift)) moveDir -= new Vector3(0, 1, 0);

            if (moveDir != Vector3.Zero)
            {
                moveDir = Vector3.Normalize(moveDir);
                Move(moveDir, _moveSpeed);
            }

            if (input.IsKeyDown(Keys.G))
            {
                _ = GameClient.Instance?.SendTcpMessage("GGG");
            }
        }
        public void Move(Vector3 direction, float amount)
        {
            Position += direction * amount;
        }
    }
}
