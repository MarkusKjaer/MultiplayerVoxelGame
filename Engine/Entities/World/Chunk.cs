using CubeEngine.Engine.MeshObject;
using CubeEngine.Engine.Window;
using OpenTK.Mathematics;
using CubeEngine.Engine.Entities.World.Mesh;
using CubeEngine.Engine.Window.Setup.Texture;

namespace CubeEngine.Engine.Entities.World
{
    public class Chunk
    {
        private ChunkData _chunkData;
        public ChunkMesh ChunkMesh { get; private set; }

        public ChunkData ChunkData 
        { 
            get
            {
                return _chunkData;
            } 
            set
            {
                _chunkData = value;
                ChunkMesh.UpdateMesh(GenChunkMesh(_chunkData));
            }
        }
        
        public Chunk(ChunkData chunkData, Material material)
        {
            _chunkData = chunkData;

            ChunkMesh = new(GenChunkMesh(chunkData), material);
        }

        private ChunkMeshInfo GenChunkMesh(ChunkData chunkData)
        {
            List<VertexPositionNormalTextureLayer> vertexPositions = [];
            List<int> indices = [];

            int vertexOffset = 0;

            for (int i = 0; i < chunkData.Voxels.GetLength(0); i++)
            {
                for (int j = 0; j < chunkData.Voxels.GetLength(1); j++)
                {
                    for (int k = 0; k < chunkData.Voxels.GetLength(2); k++)
                    {
                        if (chunkData.Voxels[i, j, k].VoxelType == Enum.VoxelType.Empty) continue;

                        int textureLayer = (int)chunkData.Voxels[i, j, k].VoxelType - 1;

                        Vector3 voxelPosition = new Vector3(i, j, k) + chunkData.Position;

                        // Define cube vertices relative to the voxel position
                        Vector3 v000 = voxelPosition + new Vector3(0, 0, 0);
                        Vector3 v100 = voxelPosition + new Vector3(1, 0, 0);
                        Vector3 v010 = voxelPosition + new Vector3(0, 1, 0);
                        Vector3 v110 = voxelPosition + new Vector3(1, 1, 0);
                        Vector3 v001 = voxelPosition + new Vector3(0, 0, 1);
                        Vector3 v101 = voxelPosition + new Vector3(1, 0, 1);
                        Vector3 v011 = voxelPosition + new Vector3(0, 1, 1);
                        Vector3 v111 = voxelPosition + new Vector3(1, 1, 1);

                        // Define normals for each face
                        Vector3 normalZPlus = new(0, 0, 1);
                        Vector3 normalZMinus = new(0, 0, -1);
                        Vector3 normalXPlus = new(1, 0, 0);
                        Vector3 normalXMinus = new(-1, 0, 0);
                        Vector3 normalYPlus = new(0, 1, 0);
                        Vector3 normalYMinus = new(0, -1, 0);

                        // Texture coordinates
                        Vector2 uv00 = new(0, 0);
                        Vector2 uv10 = new(1, 0);
                        Vector2 uv01 = new(0, 1);
                        Vector2 uv11 = new(1, 1);

                        // Front face (Z+)
                        if (GetVoxel(i, j, k + 1).VoxelType == Enum.VoxelType.Empty)
                        {
                            vertexPositions.Add(new VertexPositionNormalTextureLayer(v001, normalZPlus, uv00, textureLayer));
                            vertexPositions.Add(new VertexPositionNormalTextureLayer(v101, normalZPlus, uv10, textureLayer));
                            vertexPositions.Add(new VertexPositionNormalTextureLayer(v011, normalZPlus, uv01, textureLayer));
                            vertexPositions.Add(new VertexPositionNormalTextureLayer(v111, normalZPlus, uv11, textureLayer));

                            indices.AddRange([vertexOffset, vertexOffset + 1, vertexOffset + 2]);
                            indices.AddRange([vertexOffset + 1, vertexOffset + 3, vertexOffset + 2]);
                            vertexOffset += 4;
                        }

                        // Back face (Z-)
                        if (GetVoxel(i, j, k - 1).VoxelType == Enum.VoxelType.Empty)
                        {
                            vertexPositions.Add(new VertexPositionNormalTextureLayer(v000, normalZMinus, uv00, textureLayer));
                            vertexPositions.Add(new VertexPositionNormalTextureLayer(v010, normalZMinus, uv01, textureLayer));
                            vertexPositions.Add(new VertexPositionNormalTextureLayer(v100, normalZMinus, uv10, textureLayer));
                            vertexPositions.Add(new VertexPositionNormalTextureLayer(v110, normalZMinus, uv11, textureLayer));

                            indices.AddRange([vertexOffset, vertexOffset + 1, vertexOffset + 2]);
                            indices.AddRange([vertexOffset + 1, vertexOffset + 3, vertexOffset + 2]);
                            vertexOffset += 4;
                        }

                        // Left face (X-)
                        if (GetVoxel(i - 1, j, k).VoxelType == Enum.VoxelType.Empty)
                        {
                            vertexPositions.Add(new VertexPositionNormalTextureLayer(v000, normalXMinus, uv00, textureLayer));
                            vertexPositions.Add(new VertexPositionNormalTextureLayer(v010, normalXMinus, uv10, textureLayer));
                            vertexPositions.Add(new VertexPositionNormalTextureLayer(v001, normalXMinus, uv01, textureLayer));
                            vertexPositions.Add(new VertexPositionNormalTextureLayer(v011, normalXMinus, uv11, textureLayer));

                            indices.AddRange([vertexOffset, vertexOffset + 1, vertexOffset + 2]);
                            indices.AddRange([vertexOffset + 1, vertexOffset + 3, vertexOffset + 2]);
                            vertexOffset += 4;
                        }

                        // Right face (X+)
                        if (GetVoxel(i + 1, j, k).VoxelType == Enum.VoxelType.Empty)
                        {
                            vertexPositions.Add(new VertexPositionNormalTextureLayer(v100, normalXPlus, uv00, textureLayer));
                            vertexPositions.Add(new VertexPositionNormalTextureLayer(v110, normalXPlus, uv01, textureLayer));
                            vertexPositions.Add(new VertexPositionNormalTextureLayer(v101, normalXPlus, uv10, textureLayer));
                            vertexPositions.Add(new VertexPositionNormalTextureLayer(v111, normalXPlus, uv11, textureLayer));

                            indices.AddRange([vertexOffset, vertexOffset + 1, vertexOffset + 2]);
                            indices.AddRange([vertexOffset + 1, vertexOffset + 3, vertexOffset + 2]);
                            vertexOffset += 4;
                        }

                        // Top face (Y+)
                        if (GetVoxel(i, j + 1, k).VoxelType == Enum.VoxelType.Empty)
                        {
                            vertexPositions.Add(new VertexPositionNormalTextureLayer(v010, normalYPlus, uv00, textureLayer));
                            vertexPositions.Add(new VertexPositionNormalTextureLayer(v011, normalYPlus, uv10, textureLayer));
                            vertexPositions.Add(new VertexPositionNormalTextureLayer(v110, normalYPlus, uv01, textureLayer));
                            vertexPositions.Add(new VertexPositionNormalTextureLayer(v111, normalYPlus, uv11, textureLayer));

                            indices.AddRange([vertexOffset, vertexOffset + 1, vertexOffset + 2]);
                            indices.AddRange([vertexOffset + 1, vertexOffset + 3, vertexOffset + 2]);
                            vertexOffset += 4;
                        }

                        // Bottom face (Y-)
                        if (GetVoxel(i, j - 1, k).VoxelType == Enum.VoxelType.Empty)
                        {
                            vertexPositions.Add(new VertexPositionNormalTextureLayer(v000, normalYMinus, uv00, textureLayer));
                            vertexPositions.Add(new VertexPositionNormalTextureLayer(v001, normalYMinus, uv10, textureLayer));
                            vertexPositions.Add(new VertexPositionNormalTextureLayer(v100, normalYMinus, uv01, textureLayer));
                            vertexPositions.Add(new VertexPositionNormalTextureLayer(v101, normalYMinus, uv11, textureLayer));

                            indices.AddRange([vertexOffset, vertexOffset + 1, vertexOffset + 2]);
                            indices.AddRange([vertexOffset + 1, vertexOffset + 3, vertexOffset + 2]);
                            vertexOffset += 4;
                        }
                    }
                }
            }

            return new ChunkMeshInfo([.. vertexPositions], [.. indices]);
        }

        public Voxel GetVoxel(int x, int y, int z)
        {
            if (x >= 0 && x < ChunkData.Voxels.GetLength(0) &&
                y >= 0 && y < ChunkData.Voxels.GetLength(1) &&
                z >= 0 && z < ChunkData.Voxels.GetLength(2))
            {
                return ChunkData.Voxels[x, y, z];
            }
            else
            {
                Voxel v = new Voxel();
                v.VoxelType = Enum.VoxelType.Empty;
                return v;
            }
        }

        public void OnUpdate()
        {
            Matrix4 translation = Matrix4.CreateTranslation(_chunkData.Position);

            ChunkMesh.Model = translation;

            ChunkMesh.Update(CubeGameWindow.Instance.CurrentGameScene.ActiveCamera, CubeGameWindow.Instance.WindowWidth, CubeGameWindow.Instance.Windowheight);
        }

        public void Render()
        {
            ChunkMesh.Render();
        }
    }
}
