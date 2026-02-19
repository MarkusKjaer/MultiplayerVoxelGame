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

        private Material _material;

        public Map(int chunkSize, int maxWorldHeight, int seed, TextureArrayManager textureArrayManager)
        {
            string vertShaderPath = AssetsManager.Instance.LoadedAssets[("MapChunkShader", AssetType.VERT)].FilePath;
            string fragShaderPath = AssetsManager.Instance.LoadedAssets[("MapChunkShader", AssetType.FRAG)].FilePath;

            if (!File.Exists(vertShaderPath) || !File.Exists(fragShaderPath))
            {
                throw new FileNotFoundException("Shader file(s) not found.",
                    !File.Exists(vertShaderPath) ? vertShaderPath : fragShaderPath);
            }

            _material = new(vertShaderPath, fragShaderPath, textureArrayManager);

            GameClient.Instance.ServerMessage += OnServerMessage;
        }

        private void OnServerMessage(Packet packet)
        {
            switch (packet)
            {
                case ChunkInfoPacket chunkInfoPacket:
                    // Process through GLActionQueue to ensure OpenGL calls stay on the main thread
                    GLActionQueue.Enqueue(() => AddNewChunk(chunkInfoPacket.ChunkData));
                    break;
            }
        }

        public void AddNewChunk(ChunkData chunkData)
        {
            lock (_chunkLock)
            {
                Chunk newChunk;
                if (CurrentChunks.TryGetValue(chunkData.Position, out var existingChunk))
                {
                    existingChunk.ChunkData = chunkData; 
                    newChunk = existingChunk;
                }
                else
                {
                    newChunk = new Chunk(chunkData, _material, this);
                    CurrentChunks.Add(chunkData.Position, newChunk);
                }

                RefreshNeighbor(chunkData.Position + new Vector2(ChunkSettings.Width, 0));  // East (+X)
                RefreshNeighbor(chunkData.Position + new Vector2(-ChunkSettings.Width, 0)); // West (-X)
                RefreshNeighbor(chunkData.Position + new Vector2(0, ChunkSettings.Width));  // North (+Z)
                RefreshNeighbor(chunkData.Position + new Vector2(0, -ChunkSettings.Width)); // South (-Z)
            }
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

        public void Render()
        {
            lock (_chunkLock)
            {
                foreach (var chunk in CurrentChunks.Values)
                {
                    chunk.Render();
                }
            }
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