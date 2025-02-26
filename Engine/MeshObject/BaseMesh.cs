using CubeEngine.Engine.Window;
using OpenTK.Mathematics;
using StbImageSharp;
using OpenTK.Graphics.OpenGL;

namespace CubeEngine.Engine.MeshObject
{
    public abstract class BaseMesh<T>
    {
        protected VertexBuffer vertexBuffer;
        protected IndexBuffer indexBuffer;
        protected VertexArray vertexArray;
        protected ShaderProgram shaderProgram;

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
            //Texture
            textureID = GL.GenTexture();
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, textureID);

            // texture parameters
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Nearest);

            StbImage.stbi_set_flip_vertically_on_load(1);

            ImageResult image = ImageResult.FromStream(File.OpenRead(_material.TextureLocation), ColorComponents.RedGreenBlueAlpha);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, image.Data);

            GL.BindTexture(TextureTarget.Texture2D, 0);
        }

        public void Unload()
        {
            vertexArray?.Dispose();
            indexBuffer?.Dispose();
            vertexBuffer?.Dispose();
            shaderProgram?.Dispose();
            GL.BindTexture(TextureTarget.Texture2D, textureID);
            GL.DeleteTexture(textureID);
            GL.BindTexture(TextureTarget.Texture2D, 0);
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
            GL.BindTexture(TextureTarget.Texture2D, textureID);

            GL.BindVertexArray(vertexArray.VertexArrayHandle);

            GL.DrawElements(PrimitiveType.Triangles, _meshInfo.IndexCount, DrawElementsType.UnsignedInt, 0);
        }
    }
}
