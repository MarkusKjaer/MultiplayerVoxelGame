using CubeEngine.Engine.MeshObject;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace CubeEngine.Engine.Window
{
    public sealed class CubeGameWindow : GameWindow
    {
        private Camera _activeCamera;

        private int _windowWidth;
        private int _windowheight;

        private float test = 0f;

        private Mesh testMesh;

        public CubeGameWindow(Camera camera, int width = 1920, int height = 1080, string title = "Game1") : base(
            GameWindowSettings.Default,
            new NativeWindowSettings()
            {
                Title = title,
                ClientSize = new Vector2i(width, height),
                WindowBorder = WindowBorder.Fixed,
                StartVisible = false,
                StartFocused = true,
                API = ContextAPI.OpenGL,
                Profile = ContextProfile.Core,
                APIVersion = new Version(3, 3),
            })
        {
            _activeCamera = camera;
            _windowWidth = width;
            _windowheight = height;

            CenterWindow();
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            GL.Viewport(0, 0, e.Width, e.Height);
            base.OnResize(e);
        }

        protected override void OnLoad()
        {
            OBJFileReader oBJFileReader = new OBJFileReader();
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string parentDirectory = Directory.GetParent(baseDirectory).Parent.Parent.Parent.FullName;
            string objFilePath = Path.Combine(parentDirectory, "Models", "Suzanne.obj");

            MeshInfo meshInfo = oBJFileReader.ReadOBJFile(objFilePath);

            Material material = new(Path.Combine(parentDirectory, "Engine", "Window", "Shaders", "Cube.vert"), Path.Combine(parentDirectory, "Engine", "Window", "Shaders", "Cube.frag"), Path.Combine(parentDirectory, "Models", "ondskab.png"));
            testMesh = new(meshInfo, material);

            testMesh.Load();

            IsVisible = true;
            GL.Enable(EnableCap.DepthTest);

            base.OnLoad();
        }

        protected override void OnUnload()
        {
            testMesh.Unload();
            base.OnUnload();
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            test += 0.01f;
            Matrix4 model;
            Matrix4.CreateFromAxisAngle(new(1, 1, 0), MathHelper.DegreesToRadians(test), out model);

            testMesh.Model = model;
            testMesh.Update(_activeCamera, _windowWidth, _windowheight);

            base.OnUpdateFrame(args);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.ClearColor(new Color4(0.1f, 0.8f, 0.8f, 1f));
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            testMesh.Render();

            Context.SwapBuffers();

            base.OnRenderFrame(args);
        }
    }
}
