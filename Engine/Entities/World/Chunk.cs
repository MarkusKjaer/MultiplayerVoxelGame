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
            List<VertexPositionTextureLayer> vertexPositions = new();
            List<int> indices = new();

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

                        // Texture coordinates
                        Vector2 uv00 = new(0, 0);
                        Vector2 uv10 = new(1, 0);
                        Vector2 uv01 = new(0, 1);
                        Vector2 uv11 = new(1, 1);

                        // Front face (Z+)
                        if(GetVoxel(i, j, k - 1).VoxelType != Enum.VoxelType.Empty)
                        {
                            vertexPositions.Add(new VertexPositionTextureLayer(v001, uv00, textureLayer));
                            vertexPositions.Add(new VertexPositionTextureLayer(v101, uv10, textureLayer));
                            vertexPositions.Add(new VertexPositionTextureLayer(v011, uv01, textureLayer));
                            vertexPositions.Add(new VertexPositionTextureLayer(v111, uv11, textureLayer));

                            indices.AddRange([vertexOffset + 0, vertexOffset + 1, vertexOffset + 2]);
                            indices.AddRange([vertexOffset + 1, vertexOffset + 3, vertexOffset + 2]);

                            vertexOffset += 4;
                        }



                        // Back face (Z-)
                        if (GetVoxel(i, j, k + 1).VoxelType != Enum.VoxelType.Empty)
                        {
                            vertexPositions.Add(new VertexPositionTextureLayer(v000, uv00, textureLayer));
                            vertexPositions.Add(new VertexPositionTextureLayer(v010, uv01, textureLayer));
                            vertexPositions.Add(new VertexPositionTextureLayer(v100, uv10, textureLayer));
                            vertexPositions.Add(new VertexPositionTextureLayer(v110, uv11, textureLayer));

                            indices.AddRange([vertexOffset + 0, vertexOffset + 1, vertexOffset + 2]);
                            indices.AddRange([vertexOffset + 1, vertexOffset + 3, vertexOffset + 2]);

                            vertexOffset += 4;
                        }
                        // Left face (X+)
                        if (GetVoxel(i - 1, j, k).VoxelType != Enum.VoxelType.Empty)
                        {
                            vertexPositions.Add(new VertexPositionTextureLayer(v000, uv00, textureLayer));
                            vertexPositions.Add(new VertexPositionTextureLayer(v010, uv10, textureLayer));
                            vertexPositions.Add(new VertexPositionTextureLayer(v001, uv01, textureLayer));
                            vertexPositions.Add(new VertexPositionTextureLayer(v011, uv11, textureLayer));

                            indices.AddRange([vertexOffset + 0, vertexOffset + 1, vertexOffset + 2]);
                            indices.AddRange([vertexOffset + 1, vertexOffset + 3, vertexOffset + 2]);

                            vertexOffset += 4;
                        }
                        // Right face (X-)
                        if (GetVoxel(i + 1, j, k).VoxelType != Enum.VoxelType.Empty)
                        {
                            vertexPositions.Add(new VertexPositionTextureLayer(v100, uv00, textureLayer));
                            vertexPositions.Add(new VertexPositionTextureLayer(v110, uv01, textureLayer));
                            vertexPositions.Add(new VertexPositionTextureLayer(v101, uv10, textureLayer));
                            vertexPositions.Add(new VertexPositionTextureLayer(v111, uv11, textureLayer));

                            indices.AddRange([vertexOffset + 0, vertexOffset + 1, vertexOffset + 2]);
                            indices.AddRange([vertexOffset + 1, vertexOffset + 3, vertexOffset + 2]);

                            vertexOffset += 4;
                        }
                        // Top face (Y+)
                        if (GetVoxel(i, j - 1, k).VoxelType != Enum.VoxelType.Empty)
                        {
                            vertexPositions.Add(new VertexPositionTextureLayer(v010, uv00, textureLayer));
                            vertexPositions.Add(new VertexPositionTextureLayer(v011, uv10, textureLayer));
                            vertexPositions.Add(new VertexPositionTextureLayer(v110, uv01, textureLayer));
                            vertexPositions.Add(new VertexPositionTextureLayer(v111, uv11, textureLayer));

                            indices.AddRange([vertexOffset + 0, vertexOffset + 1, vertexOffset + 2]);
                            indices.AddRange([vertexOffset + 1, vertexOffset + 3, vertexOffset + 2]);

                            vertexOffset += 4;
                        }
                        // Bottom face (Y-)
                        if (GetVoxel(i, j + 1, k).VoxelType != Enum.VoxelType.Empty)
                        {
                            vertexPositions.Add(new VertexPositionTextureLayer(v000, uv00, textureLayer));
                            vertexPositions.Add(new VertexPositionTextureLayer(v001, uv10, textureLayer));
                            vertexPositions.Add(new VertexPositionTextureLayer(v100, uv01, textureLayer));
                            vertexPositions.Add(new VertexPositionTextureLayer(v101, uv11, textureLayer));

                            indices.AddRange([vertexOffset + 0, vertexOffset + 1, vertexOffset + 2]);
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
