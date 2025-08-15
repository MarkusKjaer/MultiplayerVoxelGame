using CubeEngine.Engine.Client.Graphics.MeshObject;
using CubeEngine.Engine.Client.Graphics.Window.Setup.Texture;
using OpenTK.Mathematics;

namespace CubeEngine.Engine.Client.World
{
    public class Map
    {
        public List<Chunk> CurrentChucnks = [];

        private int _maxChunkRendering;
        private int _chunkVoxelSize;

        private WorldGen _worldGen;

        public Map(int chunkSize, int seed, TextureArrayManager textureArrayManager)
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

            Material material = new(vertShaderPath, fragShaderPath, textureArrayManager);

            _chunkVoxelSize = chunkSize;

            List<Vector3> chunksToGen = [new(0, 0, 0),
            new(1,0,0),
            new(2,0,0),
            new(2,0,1)];

            if (seed == 0) 
            {
                _worldGen = new WorldGen();
            }
            else
            {
                _worldGen = new WorldGen(seed);
            }

            var newChunks = _worldGen.GenPartOfWorld(_chunkVoxelSize, chunksToGen);

            for (int i = 0; i < newChunks.Count; i++)
            {
                CurrentChucnks.Add(new(newChunks[i], material));
            }
        }

        public void UpdateMeshs(Camera camera, int windowWidth, int windowheight)
        {
            for (int i = 0; i < CurrentChucnks.Count; i++)
            {
                CurrentChucnks[i].OnUpdate();
            }
        }

        public void Render()
        {
            for (int i = 0; i < CurrentChucnks.Count; i++)
            {
                CurrentChucnks[i].Render();
            }
        }
    }
}
