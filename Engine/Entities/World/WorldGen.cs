using CubeEngine.Engine.Entities.World.Enum;
using OpenTK.Mathematics;

namespace CubeEngine.Engine.Entities.World
{
    public class WorldGen
    {
        int _seed;
        public WorldGen(int seed) 
        {
            _seed = seed;
            Random rand = new Random(_seed);

            
        }

        public WorldGen()
        {
            _seed = Guid.NewGuid().GetHashCode();
            Random rand = new Random(_seed);


        }

        public List<ChunkData> GenPartOfWorld(int chunkSize, List<Vector3> chunksToGenPosition)
        {
            List<ChunkData> chunks = new List<ChunkData>();

            for (int i = 0; i < chunksToGenPosition.Count; i++)
            {
                chunks.Add(GenChunk(chunkSize, chunksToGenPosition[i]));
            }

            return chunks;
        }

        private ChunkData GenChunk(int chunkSize, Vector3 chunkPosition)
        {
            ChunkData chunk = new();
            chunk.Voxels = new Voxel[chunkSize, chunkSize, chunkSize];

            for (int i = 0; i < chunkSize; i++)
            {
                for (int j = 0; j < chunkSize; j++)
                {
                    Voxel voxel = new Voxel
                    {
                        Position = new(i, 0, j),
                        VoxelType = VoxelType.Grass,
                    };
                    chunk.Voxels[i, 0, j] = voxel;
                }
            }

            return chunk;
        }


    }
}
