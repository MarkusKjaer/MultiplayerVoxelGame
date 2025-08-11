using CubeEngine.Engine.Window;
using OpenTK.Mathematics;
using StbImageSharp;
using OpenTK.Graphics.OpenGL;
using OpenTK.Windowing.Common;

namespace CubeEngine.Engine.MeshObject
{
    public abstract class BaseMesh<T> : IDisposable
    {
        private bool disposed;

        // Will be set in child classes
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
            this._meshInfo = meshInfo;
            this._material = material;
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
            GL.BindTexture(TextureTarget.Texture2DArray, textureID);
            GL.DeleteTexture(textureID);
            GL.BindTexture(TextureTarget.Texture2DArray, 0);
        }

        public virtual void Update(Camera camera, int windowWidth, int windowheight)
        {
            Matrix4 view = camera.GetCurrentView();
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(90.0f), windowWidth / (float)windowheight, 0.1f, 100.0f);
            
            shaderProgram.SetUnitform("model", Model);
            shaderProgram.SetUnitform("view", view);
            shaderProgram.SetUnitform("projection", projection);
        }

        public virtual void Render()
        {
            GL.UseProgram(shaderProgram.ShaderProgramHandle);
            GL.BindTexture(TextureTarget.Texture2DArray, _material.TextureManager.TextureID);

            GL.BindVertexArray(vertexArray.VertexArrayHandle);
        }

        ~BaseMesh()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (!disposed)
            {
                return;
            }

            vertexArray?.Dispose();
            indexBuffer?.Dispose();
            vertexBuffer?.Dispose();
            shaderProgram?.Dispose();
            GL.BindTexture(TextureTarget.Texture2DArray, textureID);
            GL.DeleteTexture(textureID);
            GL.BindTexture(TextureTarget.Texture2DArray, 0);

            disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
