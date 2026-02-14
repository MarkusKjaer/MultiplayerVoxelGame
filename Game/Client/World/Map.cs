using CubeEngine.Engine.Client.Graphics;
using CubeEngine.Engine.Client.Graphics.MeshObject;
using CubeEngine.Engine.Client.Graphics.Window.Setup.Texture;
using CubeEngine.Engine.Network;
using MultiplayerVoxelGame.Game.Resources;
using OpenTK.Mathematics;

namespace CubeEngine.Engine.Client.World
{
    public class Map
    {
        public List<Chunk> CurrentChunks = [];

        private Material _material;

        public Map(int chunkSize, int maxWorldHeight, int seed, TextureArrayManager textureArrayManager)
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string parentDirectory = Directory.GetParent(baseDirectory).FullName;
            string shadersPath = Path.Combine(parentDirectory, "Assets", "Shaders", "MapShaders");

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
                    GLActionQueue.Enqueue(() => AddNewChunk(chunkInfoPacket.ChunkData));
                    break;
                default:
                    break;
            }

        }

        public void AddNewChunk(ChunkData chunkData)
        {
            var chunkOnThisPos = CurrentChunks.Find(currentLoadedChunk => currentLoadedChunk.ChunkData.Position == chunkData.Position);

            if (chunkOnThisPos != null)
            {
                chunkOnThisPos.ChunkData = chunkData;
            }
            else
            {
                CurrentChunks.Add(new(chunkData, _material));
            }
        }

        public void UpdateMeshs(Camera camera, int windowWidth, int windowheight)
        {
            for (int i = 0; i < CurrentChunks.Count; i++)
            {
                CurrentChunks[i].OnUpdate();
            }
        }

        public void Render()
        {
            for (int i = 0; i < CurrentChunks.Count; i++)
            {
                CurrentChunks[i].Render();
            }
        }
    }
}
