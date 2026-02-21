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

        protected override bool TryHandling(VoxelGenerationContext ctx)
        {
            if (ctx.DensityAbove <= 0)
            {
                Vector3i pos = new Vector3i(ctx.X, ctx.Y, ctx.Z);
                ctx.ChunkData.SetVoxel(pos, surfaceBlockType); 
                return true;
            }
            return false;
        }
    }
}