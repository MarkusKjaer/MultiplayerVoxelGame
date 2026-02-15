using OpenTK.Mathematics;
using CubeEngine.Engine.Client.Graphics.MeshObject;
using CubeEngine.Engine.Client.Graphics.Window;
using CubeEngine.Engine.Client.World.Mesh;

namespace CubeEngine.Engine.Client.World
{
    public class Chunk
    {
        private ChunkData _chunkData;
        public ChunkMesh ChunkMesh { get; private set; }

        #region MeshData

        private static readonly Vector3[][] FaceVertices = {
            new Vector3[] { new(0,0,1), new(1,0,1), new(0,1,1), new(1,1,1) }, // Z+
            new Vector3[] { new(0,0,0), new(0,1,0), new(1,0,0), new(1,1,0) }, // Z-
            new Vector3[] { new(1,0,0), new(1,1,0), new(1,0,1), new(1,1,1) }, // X+
            new Vector3[] { new(0,0,0), new(0,1,0), new(0,0,1), new(0,1,1) }, // X-
            new Vector3[] { new(0,1,0), new(0,1,1), new(1,1,0), new(1,1,1) }, // Y+
            new Vector3[] { new(0,0,0), new(0,0,1), new(1,0,0), new(1,0,1) }  // Y-
        };

        private static readonly Vector2[][] FaceUVs = {
            new Vector2[] { new(0,0), new(1,0), new(0,1), new(1,1) }, // Z+
            new Vector2[] { new(0,0), new(0,1), new(1,0), new(1,1) }, // Z-
            new Vector2[] { new(0,0), new(0,1), new(1,0), new(1,1) }, // X+
            new Vector2[] { new(0,0), new(1,0), new(0,1), new(1,1) }, // X-
            new Vector2[] { new(0,0), new(1,0), new(0,1), new(1,1) }, // Y+
            new Vector2[] { new(0,0), new(1,0), new(0,1), new(1,1) }  // Y-
        };

        private static readonly Vector3[] FaceNormals = {
            new(0, 0, 1),  // Z+
            new(0, 0, -1), // Z-
            new(1, 0, 0),  // X+
            new(-1, 0, 0), // X-
            new(0, 1, 0),  // Y+
            new(0, -1, 0)  // Y-
        };

        private static readonly (int x, int y, int z)[][] AoOffsets = {
            new (int, int, int)[] { (-1,0,0),(0,-1,0), ( 1,0,0),(0,-1,0), (-1,0,0),(0, 1,0), ( 1,0,0),(0, 1,0) }, // Z+
            new (int, int, int)[] { (-1,0,0),(0,-1,0), (-1,0,0),(0, 1,0), ( 1,0,0),(0,-1,0), ( 1,0,0),(0, 1,0) }, // Z-
            new (int, int, int)[] { (0,-1,0),(0,0,-1), (0, 1,0),(0,0,-1), (0,-1,0),(0,0, 1), (0, 1,0),(0,0, 1) }, // X+
            new (int, int, int)[] { (0,-1,0),(0,0,-1), (0, 1,0),(0,0,-1), (0,-1,0),(0,0, 1), (0, 1,0),(0,0, 1) }, // X-
            new (int, int, int)[] { (-1,0,0),(0,0,-1), (-1,0,0),(0,0, 1), ( 1,0,0),(0,0,-1), ( 1,0,0),(0,0, 1) }, // Y+
            new (int, int, int)[] { (-1,0,0),(0,0,-1), (-1,0,0),(0,0, 1), ( 1,0,0),(0,0,-1), ( 1,0,0),(0,0, 1) }  // Y-
        };

        #endregion

        public ChunkData ChunkData
        {
            get => _chunkData;
            set
            {
                _chunkData = value;
                ChunkMesh.UpdateMesh(GenChunkMesh(_chunkData));
            }
        }

        public Chunk(ChunkData chunkData, Material material)
        {
            _chunkData = chunkData;

            ChunkMesh = new(GenChunkMesh(_chunkData), material);
        }

        private ChunkMeshInfo GenChunkMesh(ChunkData chunkData)
        {
            List<VertexPositionNormalTextureLayerAO> vertices = [];
            List<int> indices = [];
            int vertexOffset = 0;

            for (int i = 0; i < chunkData.SizeX; i++)
                for (int j = 0; j < chunkData.SizeY; j++)
                    for (int k = 0; k < chunkData.SizeZ; k++)
                    {
                        if (chunkData.GetVoxel(i, j, k).VoxelType == Enum.VoxelType.Empty)
                            continue;

                        int textureLayer = (int)chunkData.GetVoxel(i, j, k).VoxelType - 1;
                        Vector3 pos = new(i, j, k);

                        // 0: Z+, 1: Z-, 2: X+, 3: X-, 4: Y+, 5: Y-
                        for (int faceIndex = 0; faceIndex < 6; faceIndex++)
                        {
                            Vector3 normal = FaceNormals[faceIndex];

                            if (GetVoxel(i + (int)normal.X, j + (int)normal.Y, k + (int)normal.Z).VoxelType == Enum.VoxelType.Empty)
                            {
                                AddFace(vertices, indices, ref vertexOffset, pos, normal, textureLayer, i, j, k, faceIndex);
                            }
                        }

                    }

            return new ChunkMeshInfo([.. vertices], [.. indices]);
        }

        private void AddFace(
            List<VertexPositionNormalTextureLayerAO> vertices,
            List<int> indices,
            ref int vertexOffset,
            Vector3 blockPos,
            Vector3 normal,
            int textureLayer,
            int x, int y, int z,
            int faceIndex)
        {
            float[] ao = new float[4];
            var faceAoOffsets = AoOffsets[faceIndex];
            var faceVerts = FaceVertices[faceIndex];
            var faceUvs = FaceUVs[faceIndex];

            for (int v = 0; v < 4; v++)
            {
                var side1Off = faceAoOffsets[v * 2];
                var side2Off = faceAoOffsets[v * 2 + 1];
                var cornerOff = (side1Off.x + side2Off.x, side1Off.y + side2Off.y, side1Off.z + side2Off.z);

                bool side1 = GetVoxel(x + side1Off.x + (int)normal.X, y + side1Off.y + (int)normal.Y, z + side1Off.z + (int)normal.Z).VoxelType != Enum.VoxelType.Empty;
                bool side2 = GetVoxel(x + side2Off.x + (int)normal.X, y + side2Off.y + (int)normal.Y, z + side2Off.z + (int)normal.Z).VoxelType != Enum.VoxelType.Empty;
                bool corner = GetVoxel(x + cornerOff.Item1 + (int)normal.X, y + cornerOff.Item2 + (int)normal.Y, z + cornerOff.Item3 + (int)normal.Z).VoxelType != Enum.VoxelType.Empty;

                ao[v] = ComputeAO(side1, side2, corner);
            }

            // Removed the redundant (short) casts here since List<int> just converts them back to ints!
            if (ao[0] + ao[3] > ao[1] + ao[2])
            {
                // Flipped
                indices.Add(vertexOffset + 2);
                indices.Add(vertexOffset + 3);
                indices.Add(vertexOffset);
                indices.Add(vertexOffset + 3);
                indices.Add(vertexOffset + 1);
                indices.Add(vertexOffset);
            }
            else
            {
                // Normal
                indices.Add(vertexOffset + 3);
                indices.Add(vertexOffset + 1);
                indices.Add(vertexOffset + 2);
                indices.Add(vertexOffset + 2);
                indices.Add(vertexOffset + 1);
                indices.Add(vertexOffset);
            }

            for (int v = 0; v < 4; v++)
            {
                // Add the local face offset to the block's world position
                Vector3 finalPos = blockPos + faceVerts[v];
                vertices.Add(new VertexPositionNormalTextureLayerAO(finalPos, normal, faceUvs[v], textureLayer, ao[v]));
            }

            vertexOffset += 4;
        }

        private float ComputeAO(bool side1, bool side2, bool corner)
        {
            if (side1 && side2) return 0.0f;

            int value = 0;
            if (side1) value++;
            if (side2) value++;
            if (corner) value++;

            return (3 - value) / 3.0f;
        }

        public Voxel GetVoxel(int x, int y, int z)
        {
            if (x >= 0 && x < ChunkData.SizeX &&
                y >= 0 && y < ChunkData.SizeY &&
                z >= 0 && z < ChunkData.SizeZ)
            {
                return ChunkData.GetVoxel(x, y, z);
            }

            if (CubeGameWindow.Instance.CurrentGameScene.Map != null)
            {
                int globalX = (int)_chunkData.Position.X + x;
                int globalY = y; 
                int globalZ = (int)_chunkData.Position.Y + z; 

                return CubeGameWindow.Instance.CurrentGameScene.Map.GetVoxelGlobal(globalX, globalY, globalZ);
            }

            return new Voxel { VoxelType = Enum.VoxelType.Empty };
        }

        public void OnUpdate()
        {
            Matrix4 translation = Matrix4.CreateTranslation(new( _chunkData.Position.X, 0, _chunkData.Position.Y));

            ChunkMesh.Model = translation;

            ChunkMesh.Update(CubeGameWindow.Instance.CurrentGameScene.ActiveCamera, CubeGameWindow.Instance.WindowWidth, CubeGameWindow.Instance.Windowheight);
        }

        public void Render()
        {
            ChunkMesh.Render();
        }

        public void Remove()
        {
            ChunkMesh.Dispose();
        }
    }
}
