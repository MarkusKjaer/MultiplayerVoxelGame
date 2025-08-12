using CubeEngine.Engine.Entities.World.Enum;
using CubeEngine.Util;
using OpenTK.Mathematics;

namespace CubeEngine.Engine.Entities.World
{
    public class WorldGen
    {
        int _seed;
        public WorldGen(int seed) 
        {
            _seed = seed;
            Random rand = new(_seed);

            
        }

        public WorldGen()
        {
            _seed = Guid.NewGuid().GetHashCode();
            Random rand = new(_seed);


        }

        public List<ChunkData> GenPartOfWorld(int chunkSize, List<Vector3> chunksToGenPosition)
        {
            List<ChunkData> chunks = [];

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

            for (int x = 0; x < chunkSize; x++)
            {
                for (int z = 0; z < chunkSize; z++)
                {
                    // Convert chunk-local position to world position
                    float worldX = chunkPosition.X * chunkSize + x;
                    float worldZ = chunkPosition.Z * chunkSize + z;

                    // Get height from noise
                    float height = Noise.ImageHeight(worldX, worldZ) * (chunkSize - 1);
                    int groundHeight = (int)Math.Clamp(height, 0, chunkSize - 1);

                    for (int y = 0; y < chunkSize; y++)
                    {
                        VoxelType voxelType;

                        if (y == groundHeight)
                            voxelType = VoxelType.Grass;
                        else if (y < groundHeight)
                            voxelType = VoxelType.Stone;
                        else
                            voxelType = VoxelType.Empty;

                        chunk.Voxels[x, y, z] = new Voxel
                        {
                            Position = new Vector3(x, y, z),
                            VoxelType = voxelType
                        };
                    }
                }
            }

            return chunk;
        }


    }
}
