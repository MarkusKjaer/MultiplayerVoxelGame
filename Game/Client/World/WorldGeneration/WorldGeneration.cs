using CubeEngine.Engine.Client.World;
using CubeEngine.Engine.Client.World.Enum;
using CubeEngine.Util;
using MultiplayerVoxelGame.Game.Client.World.WorldGeneration.VoxelHandlers;
using OpenTK.Mathematics;

namespace MultiplayerVoxelGame.Game.Client.World.WorldGeneration
{
    public class WorldGeneration
    {
        private readonly int _seed;
        private VoxelHandler _voxelChain;

        private int _maxWorldHeight = 64;
        private int _seaLevel = 32;

        public WorldGeneration(int seed)
        {
            _seed = seed;
            BuildVoxelChain();
        }

        public WorldGeneration()
        {
            _seed = Guid.NewGuid().GetHashCode();
            BuildVoxelChain();
        }

        private void BuildVoxelChain()
        {
            _voxelChain = new SurfaceHandler();
            _voxelChain
                .SetNext(new UndergroundHandler())
                .SetNext(new WaterHandler())
                .SetNext(new AirHandler());
        }

        public List<ChunkData> GenPartOfWorld(
            int chunkSize,
            int maxWorldHeight,
            List<Vector2> chunksToGenPosition)
        {
            _maxWorldHeight = maxWorldHeight;

            List<ChunkData> chunks = new();

            foreach (var chunkPos in chunksToGenPosition)
            {
                chunks.Add(GenChunk(chunkSize, chunkPos));
            }

            return chunks;
        }

        private ChunkData GenChunk(int chunkSize, Vector2 chunkIndex)
        {
            ChunkData chunk = new(
                chunkSize,
                _maxWorldHeight,
                chunkSize,
                new Vector2(chunkIndex.X * chunkSize,
                            chunkIndex.Y * chunkSize));

            for (int x = 0; x < chunkSize; x++)
            {
                for (int z = 0; z < chunkSize; z++)
                {
                    float worldX = chunkIndex.X * chunkSize + x;
                    float worldZ = chunkIndex.Y * chunkSize + z;

                    float scale = 0.01f;

                    float height = Noise.ImageHeight(
                        worldX * scale,
                        worldZ * scale) * _maxWorldHeight;

                    int groundHeight = (int)MathF.Round(height);
                    groundHeight = Math.Clamp(
                        groundHeight,
                        0,
                        _maxWorldHeight - 1);

                    for (int y = 0; y < _maxWorldHeight; y++)
                    {
                        var context = new VoxelGenerationContext
                        {
                            Y = y,
                            GroundHeight = groundHeight,
                            SeaLevel = _seaLevel
                        };

                        VoxelType voxelType = _voxelChain.Handle(context);

                        chunk.SetVoxel(x, y, z, voxelType);
                    }
                }
            }

            return chunk;
        }
    }
}