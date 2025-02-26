using CubeEngine.Engine.Window.Setup;
using OpenTK.Mathematics;

namespace CubeEngine.Engine.Entities.World
{
    public class Map
    {
        public List<Chunk> CurrentChucnks = [];

        private int _maxChunkRendering;
        private int _chunkVoxelSize;

        private WorldGen _worldGen;

        public Map(int chunkSize, int seed, TextureArrayManager textureArrayManager)
        {
            _chunkVoxelSize = chunkSize;

            List<Vector3> chunksToGen = new List<Vector3>();
            chunksToGen.Add(new(0, 0, 0));

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
                CurrentChucnks.Add(new(newChunks[i], textureArrayManager));
            }
        }

        public void Render()
        {

        }

    }
}
