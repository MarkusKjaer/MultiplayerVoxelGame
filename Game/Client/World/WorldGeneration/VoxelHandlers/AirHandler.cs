using CubeEngine.Engine.Client.World;
using CubeEngine.Engine.Client.World.Enum;
using OpenTK.Mathematics;

namespace MultiplayerVoxelGame.Game.Client.World.WorldGeneration.VoxelHandlers
{
    public class AirHandler : VoxelHandler
    {
        public AirHandler(VoxelHandler next) : base(next)
        {
        }

        protected override bool TryHandling(VoxelGenerationContext context)
        {
            if (context.Y > context.SurfaceHeightNoise)
            {
                Vector3i pos = new Vector3i(context.X, context.Y, context.Z);
                context.ChunkData.SetVoxel(pos, VoxelType.Empty);
                return true;
            }
            return false;
        }
    }
}