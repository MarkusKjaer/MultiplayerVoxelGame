using CubeEngine.Engine.Client.Graphics;
using CubeEngine.Engine.Client.Graphics.MeshObject;
using CubeEngine.Engine.Client.Graphics.Window;
using CubeEngine.Engine.Client.World.Enum;
using CubeEngine.Engine.Client.World.Mesh;
using OpenTK.Mathematics;
using OpenTK.Windowing.Desktop;

namespace CubeEngine.Engine.Client.World
{
    public class Chunk
    {
        private ChunkData _chunkData;
        public ChunkMesh ChunkMesh { get; private set; }

        private int _meshVersion = 0;

        #region MeshData

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
                RegenerateMeshAsync();
            }
        }


        private Map _map;

        public Chunk(ChunkData chunkData, Material material, Map map)
        {
            _map = map;
            _chunkData = chunkData;
            ChunkMesh = new ChunkMesh(ChunkMeshInfo.Empty, material);
            RegenerateMeshAsync();
        }

        public Task<ChunkMeshInfo> GenerateMeshAsync()
        {
            var dataSnapshot = _chunkData;
            var neighbors = _map.GetNeighborData(dataSnapshot.Position);
            return Task.Run(() => GenChunkMesh(dataSnapshot, neighbors));
        }

        public async void RegenerateMeshAsync()
        {
            int version = ++_meshVersion;
            var dataSnapshot = _chunkData;

            var neighbors = _map.GetNeighborData(dataSnapshot.Position);

            var meshData = await Task.Run(() => GenChunkMesh(dataSnapshot, neighbors));

            GLActionQueue.Enqueue(() =>
            {
                if (version != _meshVersion) return;
                ChunkMesh.UpdateMesh(meshData);
            });
        }

        private struct MaskData
        {
            public VoxelType Type;
            public bool IsPositiveFace;
            public float AO0, AO1, AO2, AO3;

            public readonly bool Equals(MaskData other)
            {
                return Type == other.Type &&
                       IsPositiveFace == other.IsPositiveFace && 
                       AO0 == other.AO0 && AO1 == other.AO1 &&
                       AO2 == other.AO2 && AO3 == other.AO3;
            }

            public readonly bool IsEmpty => Type == VoxelType.Empty;
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

        private ChunkMeshInfo GenChunkMesh(ChunkData chunkData, ChunkData[] neighbors)
        {
            var vertices = new List<VertexPositionNormalTextureLayerAO>();
            var indices = new List<int>();
            int vertexOffset = 0;

            int[] chunkDimensions = { chunkData.SizeX, chunkData.SizeY, chunkData.SizeZ };

            // Axis: 0=X, 1=Y, 2=Z
            for (int sliceAxis = 0; sliceAxis < 3; sliceAxis++)
            {
                int axisU = (sliceAxis + 1) % 3;
                int axisV = (sliceAxis + 2) % 3;

                int[] voxelPos = new int[3];
                int[] forwardDirection = new int[3];
                forwardDirection[sliceAxis] = 1;

                int sliceWidth = chunkDimensions[axisU];
                int sliceHeight = chunkDimensions[axisV];
                MaskData[] faceMask = new MaskData[sliceWidth * sliceHeight];

                for (voxelPos[sliceAxis] = -1; voxelPos[sliceAxis] < chunkDimensions[sliceAxis];)
                {
                    int maskIndex = 0;
                    for (voxelPos[axisV] = 0; voxelPos[axisV] < chunkDimensions[axisV]; voxelPos[axisV]++)
                    {
                        for (voxelPos[axisU] = 0; voxelPos[axisU] < chunkDimensions[axisU]; voxelPos[axisU]++)
                        {
                            VoxelType currentVoxel = VoxelAt(chunkData, neighbors, voxelPos[0], voxelPos[1], voxelPos[2]);
                            VoxelType adjacentVoxel = VoxelAt(chunkData, neighbors, voxelPos[0] + forwardDirection[0], voxelPos[1] + forwardDirection[1], voxelPos[2] + forwardDirection[2]);

                            bool isPositiveFace = currentVoxel != VoxelType.Empty && adjacentVoxel == VoxelType.Empty;
                            bool isNegativeFace = adjacentVoxel != VoxelType.Empty && currentVoxel == VoxelType.Empty;

                            if (isPositiveFace || isNegativeFace)
                            {
                                VoxelType faceType = isPositiveFace ? currentVoxel : adjacentVoxel;

                                int[] solidPos = { voxelPos[0], voxelPos[1], voxelPos[2] };
                                int[] normal = { 0, 0, 0 };

                                if (isPositiveFace)
                                {
                                    normal[sliceAxis] = 1; // Facing forward
                                }
                                else
                                {
                                    solidPos[0] += forwardDirection[0];
                                    solidPos[1] += forwardDirection[1];
                                    solidPos[2] += forwardDirection[2];
                                    normal[sliceAxis] = -1;
                                }

                                int faceIndex;
                                if (sliceAxis == 0) faceIndex = 2 + (isPositiveFace ? 0 : 1); // X axis
                                else if (sliceAxis == 1) faceIndex = 4 + (isPositiveFace ? 0 : 1); // Y axis
                                else faceIndex = 0 + (isPositiveFace ? 0 : 1); // Z axis

                                // Compute AO for the 4 vertices
                                float[] ao = new float[4];
                                var currentFaceAoOffsets = AoOffsets[faceIndex];

                                int checkX = solidPos[0] + normal[0];
                                int checkY = solidPos[1] + normal[1];
                                int checkZ = solidPos[2] + normal[2];

                                for (int v = 0; v < 4; v++)
                                {
                                    var side1Off = currentFaceAoOffsets[v * 2];
                                    var side2Off = currentFaceAoOffsets[v * 2 + 1];
                                    var cornerOff = (side1Off.x + side2Off.x, side1Off.y + side2Off.y, side1Off.z + side2Off.z);

                                    bool side1 = VoxelAt(chunkData, neighbors, checkX + side1Off.x, checkY + side1Off.y, checkZ + side1Off.z) != VoxelType.Empty;
                                    bool side2 = VoxelAt(chunkData, neighbors, checkX + side2Off.x, checkY + side2Off.y, checkZ + side2Off.z) != VoxelType.Empty;
                                    bool corner = VoxelAt(chunkData, neighbors, checkX + cornerOff.Item1, checkY + cornerOff.Item2, checkZ + cornerOff.Item3) != VoxelType.Empty;

                                    ao[v] = ComputeAO(side1, side2, corner);
                                }

                                // Store in Mask
                                faceMask[maskIndex++] = new MaskData
                                {
                                    Type = faceType,
                                    IsPositiveFace = isPositiveFace,
                                    AO0 = ao[0],
                                    AO1 = ao[1],
                                    AO2 = ao[2],
                                    AO3 = ao[3]
                                };
                            }
                            else
                            {
                                faceMask[maskIndex++] = new MaskData();
                            }
                        }
                    }

                    voxelPos[sliceAxis]++;

                    maskIndex = 0;
                    for (int sliceY = 0; sliceY < sliceHeight; sliceY++)
                    {
                        for (int sliceX = 0; sliceX < sliceWidth;)
                        {
                            MaskData faceType = faceMask[maskIndex];
                            if (faceType.IsEmpty) { sliceX++; maskIndex++; continue; }

                            // Compute width of the quad
                            int quadWidth;
                            for (quadWidth = 1; sliceX + quadWidth < sliceWidth && faceMask[maskIndex + quadWidth].Equals(faceType); quadWidth++) { }

                            // Compute height of the quad
                            int quadHeight;
                            bool stopMerge = false;
                            for (quadHeight = 1; sliceY + quadHeight < sliceHeight; quadHeight++)
                            {
                                for (int widthOffset = 0; widthOffset < quadWidth; widthOffset++)
                                {
                                    if (!faceMask[maskIndex + widthOffset + quadHeight * sliceWidth].Equals(faceType))
                                    {
                                        stopMerge = true;
                                        break;
                                    }
                                }
                                if (stopMerge) break;
                            }

                            voxelPos[axisU] = sliceX;
                            voxelPos[axisV] = sliceY;

                            Vector3 quadOrigin = new Vector3(voxelPos[0], voxelPos[1], voxelPos[2]);
                            Vector3 widthVector = Vector3.Zero;
                            Vector3 heightVector = Vector3.Zero;

                            if (axisU == 0) widthVector.X = quadWidth; else if (axisU == 1) widthVector.Y = quadWidth; else widthVector.Z = quadWidth;
                            if (axisV == 0) heightVector.X = quadHeight; else if (axisV == 1) heightVector.Y = quadHeight; else heightVector.Z = quadHeight;

                            float normalSign = faceType.IsPositiveFace ? 1f : -1f;
                            Vector3 normal = Vector3.Zero;
                            if (sliceAxis == 0) normal.X = normalSign;
                            else if (sliceAxis == 1) normal.Y = normalSign;
                            else normal.Z = normalSign;

                            AddQuad(faceType, vertices, indices, ref vertexOffset, quadOrigin, widthVector, heightVector, normal, Math.Abs((int)faceType.Type) - 1);

                            for (int clearY = 0; clearY < quadHeight; clearY++)
                                for (int clearX = 0; clearX < quadWidth; clearX++)
                                    faceMask[maskIndex + clearX + clearY * sliceWidth] = new MaskData();

                            sliceX += quadWidth;
                            maskIndex += quadWidth;
                        }
                    }
                }
            }

            return new ChunkMeshInfo(vertices.ToArray(), indices.ToArray());
        }

        private VoxelType VoxelAt(ChunkData data, ChunkData[] neighbors, int x, int y, int z)
        {
            return GetVoxelWithNeighbors(x, y, z, data, neighbors);
        }

        private void AddQuad(
            MaskData maskData,
            List<VertexPositionNormalTextureLayerAO> vertices,
            List<int> indices,
            ref int vertexOffset,
            Vector3 pos,
            Vector3 du,
            Vector3 dv,
            Vector3 normal,
            int textureLayer)
        {
            Vector3 v0 = pos;
            Vector3 v1 = pos + du;
            Vector3 v2 = pos + dv;
            Vector3 v3 = pos + du + dv;

            vertices.Add(new(v0, normal, new Vector2(0, 0), textureLayer, maskData.AO0));
            vertices.Add(new(v1, normal, new Vector2(1, 0), textureLayer, maskData.AO1));
            vertices.Add(new(v2, normal, new Vector2(0, 1), textureLayer, maskData.AO2));
            vertices.Add(new(v3, normal, new Vector2(1, 1), textureLayer, maskData.AO3));

            if (maskData.AO0 + maskData.AO3 > maskData.AO1 + maskData.AO2)
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

            vertexOffset += 4;
        }


        public VoxelType GetVoxel(int x, int y, int z)
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

            return Enum.VoxelType.Empty;
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

        private VoxelType GetVoxelWithNeighbors(int x, int y, int z, ChunkData centerData, ChunkData[] neighbors)
        {
            if (y < 0 || y >= centerData.SizeY) return VoxelType.Empty;

            bool lessX = x < 0;
            bool greaterX = x >= centerData.SizeX;
            bool lessZ = z < 0;
            bool greaterZ = z >= centerData.SizeZ;

            // --- FAST PATH: Inside the center chunk ---
            if (!lessX && !greaterX && !lessZ && !greaterZ)
            {
                return centerData.GetVoxel(x, y, z);
            }

            // --- WRAP COORDINATES ---
            int wrapX = lessX ? centerData.SizeX - 1 : (greaterX ? 0 : x);
            int wrapZ = lessZ ? centerData.SizeZ - 1 : (greaterZ ? 0 : z);

            // --- BORDER PATH: Fetch from correct neighbor ---
            ChunkData targetNeighbor = null;

            if (lessX && !lessZ && !greaterZ) targetNeighbor = neighbors[0]; // X-
            else if (greaterX && !lessZ && !greaterZ) targetNeighbor = neighbors[1]; // X+
            else if (!lessX && !greaterX && lessZ) targetNeighbor = neighbors[2]; // Z-
            else if (!lessX && !greaterX && greaterZ) targetNeighbor = neighbors[3]; // Z+

            // Diagonals
            else if (lessX && lessZ) targetNeighbor = neighbors[4]; // X- Z-
            else if (greaterX && lessZ) targetNeighbor = neighbors[5]; // X+ Z-
            else if (lessX && greaterZ) targetNeighbor = neighbors[6]; // X- Z+
            else if (greaterX && greaterZ) targetNeighbor = neighbors[7]; // X+ Z+

            return targetNeighbor?.GetVoxel(wrapX, y, wrapZ) ?? VoxelType.Empty;
        }
    }
}
