using CubeEngine.Engine.Window;
using OpenTK.Mathematics;
using StbImageSharp;
using OpenTK.Graphics.OpenGL;

namespace CubeEngine.Engine.MeshObject
{
    public class Mesh
    {
        private VertexBuffer vertexBuffer;
        private IndexBuffer indexBuffer;
        private VertexArray vertexArray;
        private ShaderProgram shaderProgram;

        private int textureID;

        private MeshInfo _meshInfo;
        private Material _material;

        public Matrix4 Model {  get; set; }

        public Mesh(MeshInfo meshInfo, Material material)
        {
            this._meshInfo = meshInfo;
            this._material = material;

            vertexBuffer = new VertexBuffer(VertexPositionTexture.vertexInfo, _meshInfo.VertexCount, true);
            vertexBuffer.SetData(_meshInfo.Vertices, _meshInfo.VertexCount);

            indexBuffer = new(_meshInfo.IndexCount, true);
            indexBuffer.SetData(_meshInfo.Indices, _meshInfo.IndexCount);

            vertexArray = new VertexArray(vertexBuffer, indexBuffer);

            string vertexShaderCode = File.ReadAllText(_material.VertShaderFileLocation);
            string pixelShaderCode = File.ReadAllText(_material.FragShaderFileLocation);

            shaderProgram = new(vertexShaderCode, pixelShaderCode);
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

        public void Update(Camera camera, int windowWidth, int windowheight)
        {
            Matrix4 view = camera.GetCurrentView();
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(90.0f), windowWidth / (float)windowheight, 0.1f, 100.0f);

            shaderProgram.SetUnitform("model", Model);
            shaderProgram.SetUnitform("view", view);
            shaderProgram.SetUnitform("projection", projection);
        }

        public void Render()
        {
            GL.UseProgram(shaderProgram.ShaderProgramHandle);
            GL.BindTexture(TextureTarget.Texture2D, textureID);

            GL.BindVertexArray(vertexArray.VertexArrayHandle);

            GL.DrawElements(PrimitiveType.Triangles, _meshInfo.IndexCount, DrawElementsType.UnsignedInt, 0);
        }
    }

    public readonly struct MeshInfo
    {
        public readonly int VertexCount { get; }
        public readonly int IndexCount { get; }
        public readonly VertexPositionTexture[] Vertices { get; }
        public readonly int[] Indices {  get; }

        public MeshInfo(VertexPositionTexture[] vertexPositions, int[] indices)
        {
            Vertices = vertexPositions;
            VertexCount = Vertices.Length;

            this.Indices = indices;
            IndexCount = indices.Length;
        }
    }
}
