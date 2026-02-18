using CubeEngine.Engine.Client.Graphics.MeshObject;
using CubeEngine.Engine.Client.Graphics.Window;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace CubeEngine.Engine.Client.World.Mesh
{
    public class ChunkMesh : BaseMesh<ChunkMeshInfo>
    {
        public ChunkMesh(ChunkMeshInfo meshInfo, Material material) : base(meshInfo, material)
        {
            string vertexShaderCode = File.ReadAllText(_material.VertShaderFileLocation);
            string pixelShaderCode = File.ReadAllText(_material.FragShaderFileLocation);
            shaderProgram = new(vertexShaderCode, pixelShaderCode);

            _meshInfo = meshInfo;
            if (_meshInfo.VertexCount == 0 || _meshInfo.IndexCount == 0)
                return;

            vertexBuffer = new VertexBuffer(VertexPositionNormalTextureLayerAO.vertexInfo, _meshInfo.VertexCount, true);
            vertexBuffer.SetData(_meshInfo.Vertices, _meshInfo.VertexCount);

            indexBuffer = new(_meshInfo.IndexCount, true);
            indexBuffer.SetData(_meshInfo.Indices, _meshInfo.IndexCount);

            vertexArray = new VertexArray(vertexBuffer, indexBuffer);
        }

        public void UpdateMesh(ChunkMeshInfo newMeshInfo)
        {
            vertexArray?.Dispose();
            indexBuffer?.Dispose();
            vertexBuffer?.Dispose();

            _meshInfo = newMeshInfo;

            if (_meshInfo.VertexCount == 0 || _meshInfo.IndexCount == 0)
                return; 

            vertexBuffer = new VertexBuffer(VertexPositionNormalTextureLayerAO.vertexInfo, _meshInfo.VertexCount, true);
            vertexBuffer.SetData(_meshInfo.Vertices, _meshInfo.VertexCount);

            indexBuffer = new(_meshInfo.IndexCount, true);
            indexBuffer.SetData(_meshInfo.Indices, _meshInfo.IndexCount);

            vertexArray = new VertexArray(vertexBuffer, indexBuffer);
        }

        public override void Update(Camera camera, int windowWidth, int windowheight)
        {
            if (_meshInfo.IndexCount == 0)
                return;

            base.Update(camera, windowWidth, windowheight);
        }

        public override void Render()
        {
            if (_meshInfo.IndexCount == 0)
                return;

            base.Render();

            GL.BindTexture(TextureTarget.Texture2DArray, _material.TextureManager.TextureID);
            GL.BindVertexArray(vertexArray.VertexArrayHandle);
            GL.DrawElements(PrimitiveType.Triangles, _meshInfo.IndexCount, DrawElementsType.UnsignedInt, 0);
        }
    }

    public readonly struct ChunkMeshInfo
    {
        public static readonly ChunkMeshInfo Empty =
            new ChunkMeshInfo(Array.Empty<VertexPositionNormalTextureLayerAO>(),
                              Array.Empty<int>());
        public readonly int VertexCount { get; }
        public readonly int IndexCount { get; }
        public readonly VertexPositionNormalTextureLayerAO[] Vertices { get; }
        public readonly int[] Indices { get; }

        public ChunkMeshInfo(VertexPositionNormalTextureLayerAO[] vertexPositions, int[] indices)
        {
            Vertices = vertexPositions;
            VertexCount = Vertices.Length;

            Indices = indices;
            IndexCount = indices.Length;
        }
    }
}
