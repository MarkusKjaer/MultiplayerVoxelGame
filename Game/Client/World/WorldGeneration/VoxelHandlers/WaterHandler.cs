using CubeEngine.Engine.Client.World;
using CubeEngine.Engine.Client.World.Enum;
using OpenTK.Mathematics;

namespace MultiplayerVoxelGame.Game.Client.World.WorldGeneration.VoxelHandlers
{
    public class WaterHandler : VoxelHandler
    {
        private int _waterLevel = 1;

        public WaterHandler(VoxelHandler next, int waterLevel) : base(next)
        {
            _waterLevel = waterLevel;
        }

        protected override bool TryHandling(VoxelGenerationContext voxelGenerationContext)
        {
            if (voxelGenerationContext.Y > voxelGenerationContext.SurfaceHeightNoise && voxelGenerationContext.Y <= _waterLevel)
            {
                Vector3i pos = new Vector3i(voxelGenerationContext.X, voxelGenerationContext.Y, voxelGenerationContext.Z);
                voxelGenerationContext.ChunkData.SetVoxel(pos, VoxelType.Water);
                if (voxelGenerationContext.Y == voxelGenerationContext.SurfaceHeightNoise + 1)
                {
                    pos.Y = voxelGenerationContext.SurfaceHeightNoise;
                    voxelGenerationContext.ChunkData.SetVoxel(pos, VoxelType.Sand);
                }
                return true;
            }
            return false;
        }
    }
}