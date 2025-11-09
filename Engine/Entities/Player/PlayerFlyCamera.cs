using CubeEngine.Engine.Client;
using CubeEngine.Engine.Client.Graphics.Window;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Diagnostics;

public class PlayerFlyCamera : Camera
{
    private float _moveSpeed = 0.2f;
    private float _mouseSensitivity = 0.025f;

    public PlayerFlyCamera(Vector3 position) : base(position)
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
        if(newPitch > 89.0f || newPitch < -89.0f)
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
        if (input.IsKeyDown(Keys.Space)) moveDir += Up;
        if (input.IsKeyDown(Keys.LeftShift)) moveDir -= Up;

        if (moveDir != Vector3.Zero)
        {
            moveDir = Vector3.Normalize(moveDir);
            Move(moveDir, _moveSpeed);
        }
    }

    public void Move(Vector3 direction, float amount)
    {
        Position += direction * amount;
    }
}

