using CubeEngine.Engine.Client.World;
using CubeEngine.Engine.Client.World.Enum;
using OpenTK.Mathematics;

namespace MultiplayerVoxelGame.Game.Client.World.WorldGeneration.VoxelHandlers
{
    public class WaterHandler : VoxelHandler
    {
        private int _waterLevel;

        public WaterHandler(VoxelHandler next, int waterLevel) : base(next)
        {
            _waterLevel = waterLevel;
        }

        protected override bool TryHandling(VoxelGenerationContext ctx)
        {
            Vector3i pos = new Vector3i(ctx.X, ctx.Y, ctx.Z);

            if (ctx.Density <= 0 && ctx.Y <= _waterLevel)
            {
                ctx.ChunkData.SetVoxel(pos, VoxelType.Water);
                return true;
            }

            if (ctx.Density > 0 &&
                ctx.Y <= _waterLevel &&
                ctx.DensityAbove <= 0)
            {
                ctx.ChunkData.SetVoxel(pos, VoxelType.Sand);
                return true;
            }

            return false;
        }
    }
}