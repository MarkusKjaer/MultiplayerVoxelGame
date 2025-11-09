using CubeEngine.Engine.Client.Graphics.MeshObject;
using CubeEngine.Engine.Client.Graphics.Window.Setup.Texture;
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

            //List<Vector2> chunksToGen =
            //[
            //    new(0, 0), new(1, 0), new(2, 0), new(3, 0), new(4, 0),
            //    new(0, 1), new(1, 1), new(2, 1), new(3, 1), new(4, 1),
            //    new(0, 2), new(1, 2), new(2, 2), new(3, 2), new(4, 2),
            //    new(0, 3), new(1, 3), new(2, 3), new(3, 3), new(4, 3),
            //    new(0, 4), new(1, 4), new(2, 4), new(3, 4), new(4, 4),
            //];


            //if (seed == 0) 
            //{
            //    _worldGen = new WorldGen();
            //}
            //else
            //{
            //    _worldGen = new WorldGen(seed);
            //}

            //var newChunks = _worldGen.GenPartOfWorld(chunkSize, maxWorldHeight, chunksToGen);

            //for (int i = 0; i < newChunks.Count; i++)
            //{
            //    CurrentChunks.Add(new(newChunks[i], material));
            //}
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
