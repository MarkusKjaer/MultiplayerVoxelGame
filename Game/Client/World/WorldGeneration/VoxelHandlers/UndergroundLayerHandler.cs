using CubeEngine.Engine.Client.World.Enum;
using OpenTK.Mathematics;

namespace MultiplayerVoxelGame.Game.Client.World.WorldGeneration.VoxelHandlers
{
    public class UndergroundLayerHandler : VoxelHandler
    {
        private VoxelType _undergroundBlockType;

        public UndergroundLayerHandler(VoxelHandler next, VoxelType undergroundBlockType) : base(next)
        {
            _undergroundBlockType = undergroundBlockType;
        }

        protected override bool TryHandling(VoxelGenerationContext voxelGenerationContext)
        {
            if (voxelGenerationContext.Y < voxelGenerationContext.SurfaceHeightNoise)
            {
                Vector3i pos = new Vector3i(voxelGenerationContext.X, voxelGenerationContext.Y, voxelGenerationContext.Z);
                voxelGenerationContext.ChunkData.SetVoxel(pos, _undergroundBlockType);
                return true;
            }
            return false;
        }
    }
}
