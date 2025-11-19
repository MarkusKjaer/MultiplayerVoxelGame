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
            shaderProgram.SetUnitform("lightPos", 1000, 500, 2000); // Example light position
            shaderProgram.SetUnitform("lightColor", 1.0f, 1.0f, 1.0f); // Example light color
            shaderProgram.SetUnitform("ambient", 0.1f, 0.1f, 0.1f); // Example ambient light color
            shaderProgram.SetUnitform("objectColor", 1.0f, 1.0f, 1.0f); // Example diffuse light color
        }

        public virtual void Render()
        {
            GL.UseProgram(shaderProgram.ShaderProgramHandle);
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
