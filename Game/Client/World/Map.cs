using OpenTK.Mathematics;
using CubeEngine.Engine.Client.Graphics;
using CubeEngine.Engine.Client.Graphics.MeshObject;
using CubeEngine.Engine.Client.Graphics.Window.Setup.Texture;
using CubeEngine.Engine.Network;
using MultiplayerVoxelGame.Game.Resources;
using System.Collections.Generic;
using CubeEngine.Engine.Client.World.Enum;

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
                if (CurrentChunks.TryGetValue(chunkData.Position, out var existingChunk))
                {
                    existingChunk.ChunkData = chunkData;
                }
                else
                {
                    CurrentChunks.Add(chunkData.Position, new Chunk(chunkData, _material));
                }
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

        public Voxel GetVoxelGlobal(int globalX, int globalY, int globalZ)
        {
            Vector2 chunkPos = new Vector2(
                (float)Math.Floor(globalX / 16f),
                (float)Math.Floor(globalZ / 16f)
            );
            lock (_chunkLock)
            {
                if (CurrentChunks.TryGetValue(chunkPos, out var chunk))
                {
                    int localX = globalX - (int)chunkPos.X;
                    int localY = globalY;
                    int localZ = globalZ - (int)chunkPos.Y;
                    return chunk.GetVoxel(localX, localY, localZ);
                }
            }
            return new Voxel { VoxelType = VoxelType.Empty };

        }
    }
}