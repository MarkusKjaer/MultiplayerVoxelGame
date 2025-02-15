using CubeEngine.Engine.Window;

namespace CubeEngine.Engine.MeshObject
{
    public class Mesh
    {
        private MeshInfo meshInfo;

        public Mesh(MeshInfo meshInfo)
        {
            this.meshInfo = meshInfo;
        }

        public void Load()
        {

        }

        public void Unload()
        {

        }

        public void Render()
        {

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
