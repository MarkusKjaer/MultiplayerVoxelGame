using CubeEngine.Engine.Client.Graphics;
using CubeEngine.Engine.Client.Graphics.MeshObject;
using CubeEngine.Engine.Client.Graphics.Window.Setup.Texture;
using CubeEngine.Engine.Network;
using OpenTK.Mathematics;

namespace CubeEngine.Engine.Client.World
{
    public class Map
    {
        public List<Chunk> CurrentChunks = [];

        private int _maxChunkRendering;

        private Material _material;

        private WorldGen _worldGen;

        public Map(int chunkSize, int maxWorldHeight, int seed, TextureArrayManager textureArrayManager)
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string parentDirectory = Directory.GetParent(baseDirectory).Parent.Parent.Parent.FullName;
            string shadersPath = Path.Combine(parentDirectory, "Engine", "Client", "Graphics", "Window", "Shaders", "MapShaders");

            string vertShaderPath = Path.Combine(shadersPath, "MapChunk.vert");
            string fragShaderPath = Path.Combine(shadersPath, "MapChunk.frag");

            // Ensure the shader files exist before reading
            if (!File.Exists(vertShaderPath) || !File.Exists(fragShaderPath))
            {
                throw new FileNotFoundException("Shader file(s) not found.",
                    !File.Exists(vertShaderPath) ? vertShaderPath : fragShaderPath);
            }

            _material = new(vertShaderPath, fragShaderPath, textureArrayManager);

            GameClient.Instance.OnServerMessage += OnServerMessage;
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
            CurrentChunks.Add(new(chunkData, _material));
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
