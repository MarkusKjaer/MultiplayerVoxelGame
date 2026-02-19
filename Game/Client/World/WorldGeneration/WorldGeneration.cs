using CubeEngine.Engine.Client.World;
using CubeEngine.Engine.Client.World.Enum;
using CubeEngine.Util;
using OpenTK.Mathematics;

namespace MultiplayerVoxelGame.Game.Client.World.WorldGeneration
{
    public class WorldGeneration
    {
        int _seed;

        private int _maxWorldHeight = 64;

        public WorldGeneration(int seed) 
        {
            _seed = seed;
            Random rand = new(_seed);
        }

        public WorldGeneration()
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

        private int _seaLevel = 32;

        private ChunkData GenChunk(int chunkSize, Vector2 chunkIndex)
        {
            ChunkData chunk = new(chunkSize, _maxWorldHeight, chunkSize,
                                  new Vector2(chunkIndex.X * chunkSize, chunkIndex.Y * chunkSize));

            for (int x = 0; x < chunkSize; x++)
            {
                for (int z = 0; z < chunkSize; z++)
                {
                    float worldX = chunkIndex.X * chunkSize + x;
                    float worldZ = chunkIndex.Y * chunkSize + z;

                    float scale = 0.01f;

                    float height = Noise.ImageHeight(worldX * scale, worldZ * scale) * _maxWorldHeight;

                    int groundHeight = (int)MathF.Round(height);
                    groundHeight = Math.Clamp(groundHeight, 0, _maxWorldHeight - 1);

                    for (int y = 0; y < _maxWorldHeight; y++)
                    {
                        VoxelType voxelType;

                        if (y == groundHeight)
                        {
                            voxelType = groundHeight < _seaLevel ? VoxelType.Stone : VoxelType.Grass;
                        }
                        else if (y < groundHeight)
                        {
                            voxelType = VoxelType.Stone;
                        }
                        else if (y <= _seaLevel)
                        {
                            voxelType = VoxelType.Water;
                        }
                        else
                        {
                            voxelType = VoxelType.Empty;
                        }

                        chunk.SetVoxel(x, y, z, voxelType);
                    }
                }
            }

            return chunk;
        }
    }
}
