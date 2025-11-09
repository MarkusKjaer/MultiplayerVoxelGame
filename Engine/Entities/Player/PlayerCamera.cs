using CubeEngine.Engine.Client;
using CubeEngine.Engine.Client.Graphics.Window;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;

namespace CubeEngine.Engine.Entities.Player
{
    public class PlayerCamera : Camera
    {
        private float _mouseSensitivity = 0.025f;
        private PlayerCharacter _character;

        public PlayerCamera(Vector3 postion, PlayerCharacter playerCharacter) : base(postion)
        {
            CubeGameWindow.Instance.CursorState = CursorState.Grabbed; // Grab the cursor to prevent it from leaving the window
            _character = playerCharacter;
        }

        public override void OnUpdate()
        {
            UpdateMouseLook();
        }

        private void UpdateMouseLook()
        {
            var mouseDelta = CubeGameWindow.Instance.MouseState.Delta;

            float xoffset = -mouseDelta.X * _mouseSensitivity;
            float yoffset = -mouseDelta.Y * _mouseSensitivity; 

            float currentPitch = MathHelper.RadiansToDegrees(
                MathF.Asin(GlobalFront.Y)
            );

            float newPitch = currentPitch + yoffset;
            if (newPitch > 89.0f || newPitch < -89.0f)
                yoffset = 0;

            _character.Rotate(xoffset, 0);

            this.Rotate(0, yoffset);
        }


    }
}
