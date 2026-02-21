using CubeEngine.Engine.Client.World;
using OpenTK.Mathematics;

namespace MultiplayerVoxelGame.Game.Client.World.WorldGeneration
{
    public class VoxelGenerationContext
    {
        public ChunkData ChunkData { get; private set; }
        public int X { get; private set; }
        public int Y { get; private set; }
        public int Z { get; private set; }
        public int SurfaceHeightNoise { get; private set; }
        public Vector2i MapSeedOffset { get; private set; }

        public VoxelGenerationContext(ChunkData chunkData, int x, int y, int z, int surfaceHeightNoise, Vector2i mapSeedOffset)
        {
            ChunkData = chunkData;
            X = x;
            Y = y;
            Z = z;
            SurfaceHeightNoise = surfaceHeightNoise;
            MapSeedOffset = mapSeedOffset;
        }
    }
}
