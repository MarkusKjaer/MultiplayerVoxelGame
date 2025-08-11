using CubeEngine.Engine.MeshObject;
using CubeEngine.Engine.Window;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace CubeEngine.Engine.Entities.World.Mesh
{
    public class ChunkMesh : BaseMesh<ChunkMeshInfo>
    {
        public ChunkMesh(ChunkMeshInfo meshInfo, Material material) : base(meshInfo, material)
        {
            vertexBuffer = new VertexBuffer(VertexPositionNormalTextureLayer.vertexInfo, _meshInfo.VertexCount, true);
            vertexBuffer.SetData(_meshInfo.Vertices, _meshInfo.VertexCount);

            indexBuffer = new(_meshInfo.IndexCount, true);
            indexBuffer.SetData(_meshInfo.Indices, _meshInfo.IndexCount);

            vertexArray = new VertexArray(vertexBuffer, indexBuffer);

            string vertexShaderCode = File.ReadAllText(_material.VertShaderFileLocation);
            string pixelShaderCode = File.ReadAllText(_material.FragShaderFileLocation);

            shaderProgram = new(vertexShaderCode, pixelShaderCode);
        }

        public void UpdateMesh(ChunkMeshInfo newMeshInfo)
        {
            vertexArray?.Dispose();
            indexBuffer?.Dispose();
            vertexBuffer?.Dispose();

            _meshInfo = newMeshInfo;

            vertexBuffer = new VertexBuffer(VertexPositionNormalTextureLayer.vertexInfo, _meshInfo.VertexCount, true);
            vertexBuffer.SetData(_meshInfo.Vertices, _meshInfo.VertexCount);

            indexBuffer = new(_meshInfo.IndexCount, true);
            indexBuffer.SetData(_meshInfo.Indices, _meshInfo.IndexCount);

            vertexArray = new VertexArray(vertexBuffer, indexBuffer);
        }

        public override void Render()
        {
            base.Render();

            GL.DrawElements(PrimitiveType.Triangles, _meshInfo.IndexCount, DrawElementsType.UnsignedInt, 0);
        }
    }

    public readonly struct ChunkMeshInfo
    {
        public readonly int VertexCount { get; }
        public readonly int IndexCount { get; }
        public readonly VertexPositionNormalTextureLayer[] Vertices { get; }
        public readonly int[] Indices { get; }

        public ChunkMeshInfo(VertexPositionNormalTextureLayer[] vertexPositions, int[] indices)
        {
            Vertices = vertexPositions;
            VertexCount = Vertices.Length;

            Indices = indices;
            IndexCount = indices.Length;
        }
    }
}
