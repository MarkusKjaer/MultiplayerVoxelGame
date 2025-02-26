using CubeEngine.Engine.MeshObject;
using CubeEngine.Engine.Window;

namespace CubeEngine.Engine.Entities.World
{
    public class ChunkMesh
    {
        public ChunkMesh(MeshInfo meshInfo, Material material)
        {

        }

        public void UpdateMesh(MeshInfo newMeshInfo)
        {
            vertexArray?.Dispose();
            indexBuffer?.Dispose();
            vertexBuffer?.Dispose();

            this._meshInfo = newMeshInfo;

            vertexBuffer = new VertexBuffer(VertexPositionTexture.vertexInfo, _meshInfo.VertexCount, true);
            vertexBuffer.SetData(_meshInfo.Vertices, _meshInfo.VertexCount);

            indexBuffer = new(_meshInfo.IndexCount, true);
            indexBuffer.SetData(_meshInfo.Indices, _meshInfo.IndexCount);

            vertexArray = new VertexArray(vertexBuffer, indexBuffer);
        }
    }

    public readonly struct ChunkMeshInfo
    {
        public readonly int VertexCount { get; }
        public readonly int IndexCount { get; }
        public readonly VertexPositionTextureLayer[] Vertices { get; }
        public readonly int[] Indices { get; }

        public ChunkMeshInfo(VertexPositionTextureLayer[] vertexPositions, int[] indices)
        {
            Vertices = vertexPositions;
            VertexCount = Vertices.Length;

            this.Indices = indices;
            IndexCount = indices.Length;
        }
    }
}
