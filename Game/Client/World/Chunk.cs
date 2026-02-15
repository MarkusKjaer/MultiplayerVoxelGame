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

                        Vector3 v000 = pos + new Vector3(0, 0, 0);
                        Vector3 v100 = pos + new Vector3(1, 0, 0);
                        Vector3 v010 = pos + new Vector3(0, 1, 0);
                        Vector3 v110 = pos + new Vector3(1, 1, 0);
                        Vector3 v001 = pos + new Vector3(0, 0, 1);
                        Vector3 v101 = pos + new Vector3(1, 0, 1);
                        Vector3 v011 = pos + new Vector3(0, 1, 1);
                        Vector3 v111 = pos + new Vector3(1, 1, 1);

                        Vector2 uv00 = new(0, 0);
                        Vector2 uv10 = new(1, 0);
                        Vector2 uv01 = new(0, 1);
                        Vector2 uv11 = new(1, 1);

                        // Z+
                        if (GetVoxel(i, j, k + 1).VoxelType == Enum.VoxelType.Empty)
                        {
                            AddFace(vertices, indices, ref vertexOffset,
                                new[] { v001, v101, v011, v111 },
                                new Vector3(0, 0, 1),
                                new[] { uv00, uv10, uv01, uv11 },
                                textureLayer,
                                i, j, k,
                                new (int, int, int)[]
                                {
                                    (-1,0,0),(0,-1,0),
                                    ( 1,0,0),(0,-1,0),
                                    (-1,0,0),(0, 1,0),
                                    ( 1,0,0),(0, 1,0)
                                });
                        }
                        // Z-
                        if (GetVoxel(i, j, k - 1).VoxelType == Enum.VoxelType.Empty)
                        {
                            AddFace(vertices, indices, ref vertexOffset,
                                new[] { v000, v010, v100, v110 },
                                new Vector3(0, 0, -1),
                                new[] { uv00, uv01, uv10, uv11 },
                                textureLayer,
                                i, j, k,
                                new (int, int, int)[]
                                {
                                    (-1,0,0),(0,-1,0),
                                    (-1,0,0),(0, 1,0),
                                    ( 1,0,0),(0,-1,0),
                                    ( 1,0,0),(0, 1,0)
                                });
                        }
                        // X+
                        if (GetVoxel(i + 1, j, k).VoxelType == Enum.VoxelType.Empty)
                        {
                            AddFace(vertices, indices, ref vertexOffset,
                                new[] { v100, v110, v101, v111 },
                                new Vector3(1, 0, 0),
                                new[] { uv00, uv01, uv10, uv11 },
                                textureLayer,
                                i, j, k,
                                new (int, int, int)[]
                                {
                                    (0,-1,0),(0,0,-1),
                                    (0, 1,0),(0,0,-1),
                                    (0,-1,0),(0,0, 1),
                                    (0, 1,0),(0,0, 1)
                                });
                        }
                        // X-
                        if (GetVoxel(i - 1, j, k).VoxelType == Enum.VoxelType.Empty)
                        {
                            AddFace(vertices, indices, ref vertexOffset,
                                new[] { v000, v010, v001, v011 },
                                new Vector3(-1, 0, 0),
                                new[] { uv00, uv10, uv01, uv11 },
                                textureLayer,
                                i, j, k,
                                new (int, int, int)[]
                                {
                                    (0,-1,0),(0,0,-1),
                                    (0, 1,0),(0,0,-1),
                                    (0,-1,0),(0,0, 1),
                                    (0, 1,0),(0,0, 1)
                                });
                        }
                        // Y+
                        if (GetVoxel(i, j + 1, k).VoxelType == Enum.VoxelType.Empty)
                        {
                            AddFace(vertices, indices, ref vertexOffset,
                                new[] { v010, v011, v110, v111 },
                                new Vector3(0, 1, 0),
                                new[] { uv00, uv10, uv01, uv11 },
                                textureLayer,
                                i, j, k,
                                new (int, int, int)[]
                                {
                                    (-1,0,0),(0,0,-1),
                                    (-1,0,0),(0,0, 1),
                                    ( 1,0,0),(0,0,-1),
                                    ( 1,0,0),(0,0, 1)
                                });
                        }
                        // Y-
                        if (GetVoxel(i, j - 1, k).VoxelType == Enum.VoxelType.Empty)
                        {
                            AddFace(vertices, indices, ref vertexOffset,
                                new[] { v000, v001, v100, v101 },
                                new Vector3(0, -1, 0),
                                new[] { uv00, uv10, uv01, uv11 },
                                textureLayer,
                                i, j, k,
                                new (int, int, int)[]
                                {
                                    (-1,0,0),(0,0,-1),
                                    (-1,0,0),(0,0, 1),
                                    ( 1,0,0),(0,0,-1),
                                    ( 1,0,0),(0,0, 1)
                                });
                        }

                    }

            return new ChunkMeshInfo([.. vertices], [.. indices]);
        }

        private void AddFace(
            List<VertexPositionNormalTextureLayerAO> vertices,
            List<int> indices,
            ref int vertexOffset,
            Vector3[] positions,
            Vector3 normal,
            Vector2[] uvs,
            int textureLayer,
            int x, int y, int z,
            (int x, int y, int z)[] aoOffsets)
        {
            float[] ao = new float[4];

            for (int v = 0; v < 4; v++)
            {
                var side1Off = aoOffsets[v * 2];
                var side2Off = aoOffsets[v * 2 + 1];

                var cornerOff = (side1Off.x + side2Off.x, side1Off.y + side2Off.y, side1Off.z + side2Off.z);

                bool side1 = GetVoxel(x + side1Off.x + (int)normal.X, y + side1Off.y + (int)normal.Y, z + side1Off.z + (int)normal.Z).VoxelType != Enum.VoxelType.Empty;
                bool side2 = GetVoxel(x + side2Off.x + (int)normal.X, y + side2Off.y + (int)normal.Y, z + side2Off.z + (int)normal.Z).VoxelType != Enum.VoxelType.Empty;
                bool corner = GetVoxel(x + cornerOff.Item1 + (int)normal.X, y + cornerOff.Item2 + (int)normal.Y, z + cornerOff.Item3 + (int)normal.Z).VoxelType != Enum.VoxelType.Empty;

                ao[v] = ComputeAO(side1, side2, corner);
            }

            // Flips the quad triangulation to ensure smooth AO gradients
            if (ao[0] + ao[3] > ao[1] + ao[2])
            {
                //flipped
                indices.Add((short)(vertexOffset + 2));
                indices.Add((short)(vertexOffset + 3));
                indices.Add(vertexOffset);
                indices.Add((short)(vertexOffset + 3));
                indices.Add((short)(vertexOffset + 1));
                indices.Add(vertexOffset);
            }
            else
            {
                indices.Add((short)(vertexOffset + 3));
                indices.Add((short)(vertexOffset + 1));
                indices.Add((short)(vertexOffset + 2));
                indices.Add((short)(vertexOffset + 2));
                indices.Add((short)(vertexOffset + 1));
                indices.Add(vertexOffset);
            }

            for (int v = 0; v < 4; v++)
            {
                vertices.Add(new VertexPositionNormalTextureLayerAO(positions[v], normal, uvs[v], textureLayer, ao[v]));
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
            else
            {
                Voxel v = new Voxel();
                v.VoxelType = Enum.VoxelType.Empty;
                return v;
            }
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
