using CubeEngine.Engine.MeshObject;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System.IO;

namespace CubeEngine.Engine.Window
{
    public sealed class CubeGameWindow : GameWindow
    {
        private VertexBuffer vertexBuffer;
        private IndexBuffer indexBuffer;
        private VertexArray vertexArray;
        private ShaderProgram shaderProgram;

        private Camera activeCamera;

        private int vertexCount;
        private int indexCount;

        private int windowWidth;
        private int windowheight;

        private float test = 0f;

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
            activeCamera = camera;
            windowWidth = width;
            windowheight = height;

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

            IsVisible = true;
            GL.Enable(EnableCap.DepthTest);

            vertexCount += meshInfo.vertexCount;

            indexCount += meshInfo.indexCount;

            vertexBuffer = new VertexBuffer(VertexPositionTexture.vertexInfo, meshInfo.vertexCount, true);
            vertexBuffer.SetData(meshInfo.vertices, meshInfo.vertexCount);

            indexBuffer = new(meshInfo.indexCount, true);
            indexBuffer.SetData(meshInfo.indices, meshInfo.indexCount);

            vertexArray = new VertexArray(vertexBuffer, indexBuffer);

            string vertexShaderCode = File.ReadAllText(Path.Combine(parentDirectory, "Engine", "Window", "Shaders", "Cube.vert"));
            string pixelShaderCode = File.ReadAllText(Path.Combine(parentDirectory, "Engine", "Window", "Shaders", "Cube.frag"));

            shaderProgram = new(vertexShaderCode, pixelShaderCode);

            Matrix4 model = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(0f));
            Matrix4 view = activeCamera.GetCurrentView();


            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(90.0f), windowWidth / (float)windowheight, 0.1f, 100.0f);

            shaderProgram.SetUnitform("model", model);
            shaderProgram.SetUnitform("view", view);
            shaderProgram.SetUnitform("projection", projection);

            base.OnLoad();
        }

        protected override void OnUnload()
        {
            vertexArray?.Dispose();
            indexBuffer?.Dispose();
            vertexBuffer?.Dispose();
            shaderProgram?.Dispose();

            base.OnUnload();
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            test += 0.01f;
            Matrix4 model;
            Matrix4.CreateFromAxisAngle(new(1, 1, 0), MathHelper.DegreesToRadians(test), out model);
            shaderProgram.SetUnitform("model", model);
            base.OnUpdateFrame(args);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.ClearColor(new Color4(0.1f, 0.8f, 0.8f, 1f));
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);


            GL.UseProgram(shaderProgram.ShaderProgramHandle);
            
            GL.BindVertexArray(vertexArray.VertexArrayHandle);

            GL.DrawElements(PrimitiveType.Triangles, indexCount, DrawElementsType.UnsignedInt, 0);


            Context.SwapBuffers();

            base.OnRenderFrame(args);
        }

        private void LoadMesh(Mesh mesh)
        {

        }
        private void UnLoadMesh(Mesh mesh)
        {

        }
    }
}
