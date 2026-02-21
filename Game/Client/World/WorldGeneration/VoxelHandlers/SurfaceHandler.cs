using CubeEngine.Engine.Client.World;
using CubeEngine.Engine.Client.World.Enum;
using OpenTK.Mathematics;

namespace MultiplayerVoxelGame.Game.Client.World.WorldGeneration.VoxelHandlers
{
    public class SurfaceHandler : VoxelHandler
    {
        public VoxelType surfaceBlockType;

        public SurfaceHandler(VoxelType surfaceBlockType, VoxelHandler next) : base(next)
        {
            this.surfaceBlockType = surfaceBlockType;
        }

        protected override bool TryHandling(VoxelGenerationContext voxelGenerationContext)
        {
            if (voxelGenerationContext.Y == voxelGenerationContext.SurfaceHeightNoise)
            {
                Vector3i pos = new Vector3i(voxelGenerationContext.X, voxelGenerationContext.Y, voxelGenerationContext.Z);
                voxelGenerationContext.ChunkData.SetVoxel(pos, surfaceBlockType);
                return true;
            }
            return false;
        }
    }
}