using CubeEngine.Engine.Client.Graphics;
using CubeEngine.Engine.Client.Graphics.MeshObject;
using CubeEngine.Engine.Client.Graphics.Window.Setup.Texture;
using CubeEngine.Engine.Client.World.Enum;
using CubeEngine.Engine.Network;
using MultiplayerVoxelGame.Game.Resources;
using MultiplayerVoxelGame.Util.Settings;
using OpenTK.Mathematics;
using System.Collections.Generic;

namespace CubeEngine.Engine.Client.World
{
    public class Map
    {
        private readonly object _chunkLock = new object();

        public Dictionary<Vector2, Chunk> CurrentChunks = new();

        private Material _solidMaterial;
        private Material _waterMaterial;

        public Map(int chunkSize, int maxWorldHeight, int seed, TextureArrayManager solidTextureArrayManager, TextureManager waterTextureManager)
        {
            string solidVertShaderPath = AssetsManager.Instance.LoadedAssets[("MapChunkShader", AssetType.VERT)].FilePath;
            string solidFragShaderPath = AssetsManager.Instance.LoadedAssets[("MapChunkShader", AssetType.FRAG)].FilePath;
            string waterVertShaderPath = AssetsManager.Instance.LoadedAssets[("WaterChunkShader", AssetType.VERT)].FilePath;
            string waterFragShaderPath = AssetsManager.Instance.LoadedAssets[("WaterChunkShader", AssetType.FRAG)].FilePath;

            if (!File.Exists(solidVertShaderPath) || !File.Exists(solidFragShaderPath))
            {
                throw new FileNotFoundException("Shader file(s) not found.",
                    !File.Exists(solidVertShaderPath) ? solidVertShaderPath : solidFragShaderPath);
            }

            _solidMaterial = new(solidVertShaderPath, solidFragShaderPath, solidTextureArrayManager);
            _waterMaterial = new(waterVertShaderPath, waterFragShaderPath, waterTextureManager);

            GameClient.Instance.ServerMessage += OnServerMessage;
        }

        private void OnServerMessage(Packet packet)
        {
            switch (packet)
            {
                case ChunkInfoPacket chunkInfoPacket:
                    Task.Run(() => AddNewChunk(chunkInfoPacket.ChunkData));
                    break;
            }
        }

        public void AddNewChunk(ChunkData chunkData)
        {
            Chunk newChunk;
            bool requiresMeshGen = true;

            lock (_chunkLock)
            {
                if (CurrentChunks.TryGetValue(chunkData.Position, out var existingChunk))
                {
                    existingChunk.ChunkData = chunkData;
                    newChunk = existingChunk;
                }
                else
                {
                    newChunk = new Chunk(chunkData, _solidMaterial, _waterMaterial, this);
                    CurrentChunks.Add(chunkData.Position, newChunk);
                }
            }

            if (requiresMeshGen)
            {
                newChunk.RegenerateMeshAsync();
            }

            RefreshNeighbor(chunkData.Position + new Vector2(ChunkSettings.Width, 0));  // East
            RefreshNeighbor(chunkData.Position + new Vector2(-ChunkSettings.Width, 0)); // West
            RefreshNeighbor(chunkData.Position + new Vector2(0, ChunkSettings.Width));  // North
            RefreshNeighbor(chunkData.Position + new Vector2(0, -ChunkSettings.Width)); // South
        }

        private void RefreshNeighbor(Vector2 position)
        {
            if (CurrentChunks.TryGetValue(position, out var neighbor))
            {
                neighbor.RegenerateMeshAsync();
            }
        }

        public void UpdateMeshs(Camera camera, int windowWidth, int windowheight)
        {
            lock (_chunkLock)
            {
                foreach (var chunk in CurrentChunks.Values)
                {
                    chunk.OnUpdate();
                }
            }
        }

        public void Render(Vector3 cameraPosition)
        {
            List<Chunk> chunkList;
            lock (_chunkLock)
            {
                chunkList = new List<Chunk>(CurrentChunks.Values);
            }

            // Draw all solid meshes first 
            foreach (var chunk in chunkList)
                chunk.RenderSolid();

            // Sort water chunks back-to-front from camera
            chunkList.Sort((a, b) =>
            {
                var posA = new Vector3(a.ChunkData.Position.X, 0, a.ChunkData.Position.Y);
                var posB = new Vector3(b.ChunkData.Position.X, 0, b.ChunkData.Position.Y);
                float distA = (posA - cameraPosition).LengthSquared;
                float distB = (posB - cameraPosition).LengthSquared;
                return distB.CompareTo(distA); // farthest first
            });

            foreach (var chunk in chunkList)
                chunk.RenderTransparent();
        }

        public VoxelType GetVoxelGlobal(int globalX, int globalY, int globalZ)
        {
            const int CHUNK_SIZE = ChunkSettings.Width;

            int chunkX = (int)Math.Floor(globalX / (float)CHUNK_SIZE);
            int chunkZ = (int)Math.Floor(globalZ / (float)CHUNK_SIZE);

            int localX = globalX - chunkX * CHUNK_SIZE;
            int localZ = globalZ - chunkZ * CHUNK_SIZE;
            int localY = globalY;

            if (localX < 0 || localX >= CHUNK_SIZE ||
                localZ < 0 || localZ >= CHUNK_SIZE ||
                localY < 0 || localY >= 80)
            {
                return VoxelType.Empty;
            }

            Vector2 chunkPos = new Vector2(chunkX, chunkZ);

            lock (_chunkLock)
            {
                if (CurrentChunks.TryGetValue(chunkPos, out var chunk))
                {
                    return chunk.ChunkData.GetVoxel(localX, localY, localZ);
                }
            }

            return VoxelType.Empty;
        }

        public ChunkData[] GetNeighborData(Vector2 centerPosition)
        {
            ChunkData[] neighbors = new ChunkData[8];
            float w = ChunkSettings.Width;

            lock (_chunkLock)
            {
                // Direct Sides
                if (CurrentChunks.TryGetValue(centerPosition + new Vector2(-w, 0), out var c0)) neighbors[0] = c0.ChunkData; // X-
                if (CurrentChunks.TryGetValue(centerPosition + new Vector2(w, 0), out var c1)) neighbors[1] = c1.ChunkData; // X+
                if (CurrentChunks.TryGetValue(centerPosition + new Vector2(0, -w), out var c2)) neighbors[2] = c2.ChunkData; // Z-
                if (CurrentChunks.TryGetValue(centerPosition + new Vector2(0, w), out var c3)) neighbors[3] = c3.ChunkData; // Z+

                // Diagonals (Crucial for corner AO across chunk borders)
                if (CurrentChunks.TryGetValue(centerPosition + new Vector2(-w, -w), out var c4)) neighbors[4] = c4.ChunkData; // X- Z-
                if (CurrentChunks.TryGetValue(centerPosition + new Vector2(w, -w), out var c5)) neighbors[5] = c5.ChunkData; // X+ Z-
                if (CurrentChunks.TryGetValue(centerPosition + new Vector2(-w, w), out var c6)) neighbors[6] = c6.ChunkData; // X- Z+
                if (CurrentChunks.TryGetValue(centerPosition + new Vector2(w, w), out var c7)) neighbors[7] = c7.ChunkData; // X+ Z+
            }

            return neighbors;
        }

        public void RemoveOutOfRangeChunks(Vector3 playerPosition, int chunkRadius)
        {
            int playerChunkX = (int)MathF.Floor(playerPosition.X / ChunkSettings.Width);
            int playerChunkZ = (int)MathF.Floor(playerPosition.Z / ChunkSettings.Width);

            List<Vector2> chunksToRemove = new();

            lock (_chunkLock)
            {
                foreach (var kvp in CurrentChunks)
                {
                    Vector2 worldPos = kvp.Key;

                    int chunkX = (int)(worldPos.X / ChunkSettings.Width);
                    int chunkZ = (int)(worldPos.Y / ChunkSettings.Width);

                    int dx = chunkX - playerChunkX;
                    int dz = chunkZ - playerChunkZ;

                    if (dx * dx + dz * dz > chunkRadius * chunkRadius)
                    {
                        chunksToRemove.Add(worldPos);
                    }
                }

                foreach (var pos in chunksToRemove)
                {
                    if (CurrentChunks.TryGetValue(pos, out var chunk))
                    {
                        chunk.Remove(); 
                        CurrentChunks.Remove(pos);
                    }
                }
            }
        }
    }
}