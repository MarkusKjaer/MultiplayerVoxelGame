using CubeEngine.Engine.Client.World.Enum;
using CubeEngine.Util;
using OpenTK.Mathematics;

namespace CubeEngine.Engine.Client.World
{
    public class WorldGen
    {
        int _seed;

        private int _maxWorldHeight = 64;

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

        public List<ChunkData> GenPartOfWorld(int chunkSize, int maxWorldHeight, List<Vector2> chunksToGenPosition)
        {
            _maxWorldHeight = maxWorldHeight;

            List<ChunkData> chunks = [];

            for (int i = 0; i < chunksToGenPosition.Count; i++)
            {
                chunks.Add(GenChunk(chunkSize, chunksToGenPosition[i]));
            }

            return chunks;
        }

        private ChunkData GenChunk(int chunkSize, Vector2 chunkIndex)
        {
            ChunkData chunk = new(chunkSize, _maxWorldHeight, chunkSize, new Vector2(chunkIndex.X * chunkSize, chunkIndex.Y * chunkSize));

            for (int x = 0; x < chunkSize; x++)
            {
                for (int z = 0; z < chunkSize; z++)
                {
                    float worldX = chunkIndex.X * chunkSize + x;
                    float worldZ = chunkIndex.Y * chunkSize + z;

                    float scale = 0.01f;

                    float height = Noise.ImageHeight(
                        worldX * scale,
                        worldZ * scale
                    ) * _maxWorldHeight;

                    int groundHeight = (int)MathF.Round(height);
                    groundHeight = Math.Clamp(groundHeight, 0, _maxWorldHeight - 1);

                    for (int y = 0; y < _maxWorldHeight; y++)
                    {
                        VoxelType voxelType = y == groundHeight ? VoxelType.Grass :
                                              y < groundHeight ? VoxelType.Stone :
                                              VoxelType.Empty;

                        chunk.SetVoxel(x, y, z, voxelType);
                    }
                }
            }

            return chunk;
        }



    }
}
