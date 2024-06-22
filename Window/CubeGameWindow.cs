using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System;
using System.Diagnostics;
using static System.Net.Mime.MediaTypeNames;


namespace CubeEngine.Window
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
            this.activeCamera = camera;
            this.windowWidth = width;
            this.windowheight = height;

            CenterWindow();
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            GL.Viewport(0, 0, e.Width, e.Height);
            base.OnResize(e);
        }

        protected override void OnLoad()
        {
            IsVisible = true;
            GL.Enable(EnableCap.DepthTest);

            VertexPositionColor[] vertices = new VertexPositionColor[8];
            this.vertexCount += 8;

            Color4 red = Color4.Red;
            Color4 blue = Color4.Blue;

            vertices[0] = new VertexPositionColor(new Vector3(-0.5f, 0.5f, -0.5f), red);
            vertices[1] = new VertexPositionColor(new Vector3(0.5f, 0.5f, -0.5f), blue);
            vertices[2] = new VertexPositionColor(new Vector3(0.5f, -0.5f, -0.5f), red);
            vertices[3] = new VertexPositionColor(new Vector3(-0.5f, -0.5f, -0.5f), blue);
            vertices[4] = new VertexPositionColor(new Vector3(-0.5f, 0.5f, 0.5f), red);
            vertices[5] = new VertexPositionColor(new Vector3(0.5f, 0.5f, 0.5f), blue);
            vertices[6] = new VertexPositionColor(new Vector3(0.5f, -0.5f, 0.5f), red);
            vertices[7] = new VertexPositionColor(new Vector3(-0.5f, -0.5f, 0.5f), blue);


            int[] indices = [
                // Front face
                0, 1, 2,
                2, 3, 0,
                // Back face
                4, 5, 6,
                6, 7, 4,
                // Left face
                0, 4, 7,
                7, 3, 0,
                // Right face
                1, 5, 6,
                6, 2, 1,
                // Top face
                3, 7, 6,
                6, 2, 3,
                // Bottom face
                0, 4, 5,
                5, 1, 0
            ];
            this.indexCount += 36;

            this.vertexBuffer = new VertexBuffer(VertexPositionColor.vertexInfo, vertices.Length, true);
            this.vertexBuffer.SetData(vertices, vertices.Length);

            this.indexBuffer = new(indices.Length, true);
            this.indexBuffer.SetData(indices, indices.Length);

            this.vertexArray = new VertexArray(this.vertexBuffer, this.indexBuffer);

            string vertexShaderCode = File.ReadAllText("C:\\CSharp_DEV\\OpenTKCube\\OpenTKCube\\Window\\Shaders\\Cube.vert");
            string pixelShaderCode = File.ReadAllText("C:\\CSharp_DEV\\OpenTKCube\\OpenTKCube\\Window\\Shaders\\Cube.frag");

            this.shaderProgram = new(vertexShaderCode, pixelShaderCode);

            Matrix4 model = Matrix4.CreateRotationX(MathHelper.DegreesToRadians(0f));
            Matrix4 view = activeCamera.GetCurrentView();


            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(90.0f), (float)this.windowWidth / (float)this.windowheight, 0.1f, 100.0f);

            this.shaderProgram.SetUnitform("model", model);
            this.shaderProgram.SetUnitform("view", view);
            this.shaderProgram.SetUnitform("projection", projection);
            


            base.OnLoad();
        }

        protected override void OnUnload()
        {
            this.vertexArray?.Dispose();
            this.indexBuffer?.Dispose();
            this.vertexBuffer?.Dispose();
            this.shaderProgram?.Dispose();

            base.OnUnload();
        }

        protected override void OnUpdateFrame(FrameEventArgs args)
        {
            test += 0.01f;
            Matrix4 model = Matrix4.CreateRotationY(MathHelper.DegreesToRadians(test));
            this.shaderProgram.SetUnitform("model", model);
            base.OnUpdateFrame(args);
        }

        protected override void OnRenderFrame(FrameEventArgs args)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.ClearColor(new Color4(0.1f, 0.8f, 0.8f, 1f));
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.UseProgram(this.shaderProgram.ShaderProgramHandle);

            GL.BindVertexArray(this.vertexArray.VertexArrayHandle);
            GL.DrawElements(PrimitiveType.Triangles, this.indexCount, DrawElementsType.UnsignedInt, 0);

            
            this.Context.SwapBuffers();

            base.OnRenderFrame(args);
        }
    }
}
