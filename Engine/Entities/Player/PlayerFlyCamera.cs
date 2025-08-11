using CubeEngine.Engine;
using CubeEngine.Engine.Window;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

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

        float xoffset = mouseDelta.X * _mouseSensitivity;
        float yoffset = -mouseDelta.Y * _mouseSensitivity;

        Rotate(xoffset, yoffset);

        // Movement
        KeyboardState input = CubeGameWindow.Instance.KeyboardState;
        Vector3 moveDir = Vector3.Zero;

        if (input.IsKeyDown(Keys.W)) moveDir += front;
        if (input.IsKeyDown(Keys.S)) moveDir -= front;
        if (input.IsKeyDown(Keys.A)) moveDir -= right;
        if (input.IsKeyDown(Keys.D)) moveDir += right;
        if (input.IsKeyDown(Keys.Space)) moveDir += up;
        if (input.IsKeyDown(Keys.LeftShift)) moveDir -= up;

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

