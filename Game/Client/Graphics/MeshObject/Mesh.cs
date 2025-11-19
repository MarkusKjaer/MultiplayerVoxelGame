using CubeEngine.Engine.Client.Graphics.Window;
using OpenTK.Graphics.OpenGL;

namespace CubeEngine.Engine.Client.Graphics.MeshObject
{
    public class Mesh : BaseMesh<MeshInfo>
    {
        public Mesh(MeshInfo meshInfo, Material material) : base(meshInfo, material)
        {
            vertexBuffer = new VertexBuffer(VertexPositionNormalTexture.vertexInfo, _meshInfo.VertexCount, true);
            vertexBuffer.SetData(_meshInfo.Vertices, _meshInfo.VertexCount);

            indexBuffer = new(_meshInfo.IndexCount, true);
            indexBuffer.SetData(_meshInfo.Indices, _meshInfo.IndexCount);

            vertexArray = new VertexArray(vertexBuffer, indexBuffer);

            string vertexShaderCode = File.ReadAllText(_material.VertShaderFileLocation);
            string pixelShaderCode = File.ReadAllText(_material.FragShaderFileLocation);

            shaderProgram = new(vertexShaderCode, pixelShaderCode);
        }

        public override void Render()
        {
            base.Render();

            GL.BindTexture(TextureTarget.Texture2D, _material.TextureManager.TextureID);

            GL.BindVertexArray(vertexArray.VertexArrayHandle);

            GL.DrawElements(PrimitiveType.Triangles, _meshInfo.IndexCount, DrawElementsType.UnsignedInt, 0);
        }
    }

    public readonly struct MeshInfo
    {
        public readonly int VertexCount { get; }
        public readonly int IndexCount { get; }
        public readonly VertexPositionNormalTexture[] Vertices { get; }
        public readonly int[] Indices {  get; }

        public MeshInfo(VertexPositionNormalTexture[] vertexPositions, int[] indices)
        {
            Vertices = vertexPositions;
            VertexCount = Vertices.Length;

            Indices = indices;
            IndexCount = indices.Length;
        }
    }
}
