using OpenTK.Mathematics;
using StbImageSharp;
using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.Common;
using CubeEngine.Engine.Client.Graphics.Window;

namespace CubeEngine.Engine.Client.Graphics.MeshObject
{
    public abstract class BaseMesh<T> : IDisposable
    {
        private bool disposed;

        protected VertexBuffer vertexBuffer = default!;
        protected IndexBuffer indexBuffer = default!;
        protected VertexArray vertexArray = default!;
        protected ShaderProgram shaderProgram = default!;
        protected int textureID;

        protected T _meshInfo;
        protected Material _material;

        public Matrix4 Model { get; set; }

        public BaseMesh(T meshInfo, Material material)
        {
            _meshInfo = meshInfo;
            _material = material;
        }

        public void Load()
        {
            
        }

        public void Unload()
        {
            vertexArray?.Dispose();
            indexBuffer?.Dispose();
            vertexBuffer?.Dispose();
            shaderProgram?.Dispose();

            if (textureID != 0)
            {
                GL.DeleteTexture(textureID);
                textureID = 0;
            }
        }

        public virtual void Update(Camera camera, int windowWidth, int windowheight)
        {
            Matrix4 view = camera.GetCurrentView();
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(
                MathHelper.DegreesToRadians(90.0f),
                windowWidth / (float)windowheight,
                0.1f, 1000.0f
            );

            shaderProgram.SetUniform("model", Model);
            shaderProgram.SetUniform("view", view);
            shaderProgram.SetUniform("projection", projection);

        }

        public virtual void Render()
        {
            GL.UseProgram(shaderProgram.ShaderProgramHandle);
        }

        ~BaseMesh()
        {
            GLActionQueue.Enqueue(() => Dispose());
        }

        public void Dispose()
        {
            if (disposed)
                return;

            vertexArray?.Dispose();
            indexBuffer?.Dispose();
            vertexBuffer?.Dispose();
            shaderProgram?.Dispose();

            if (textureID != 0)
            {
                GL.DeleteTexture(textureID);
                textureID = 0;
            }

            disposed = true;
            GC.SuppressFinalize(this);
        }
    }

}
