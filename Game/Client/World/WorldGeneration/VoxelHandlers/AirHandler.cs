using CubeEngine.Engine.Client.World;
using CubeEngine.Engine.Client.World.Enum;
using OpenTK.Mathematics;

namespace MultiplayerVoxelGame.Game.Client.World.WorldGeneration.VoxelHandlers
{
    public class AirHandler : VoxelHandler
    {
        public AirHandler(VoxelHandler next) : base(next) { }

        protected override bool TryHandling(VoxelGenerationContext ctx)
        {
            if (ctx.Density <= 0)
            {
                Vector3i pos = new Vector3i(ctx.X, ctx.Y, ctx.Z);
                ctx.ChunkData.SetVoxel(pos, VoxelType.Empty);
                return true;
            }
            return false;
        }
    }
}