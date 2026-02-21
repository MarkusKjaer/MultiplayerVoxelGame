using CubeEngine.Engine.Client.World;
using CubeEngine.Engine.Client.World.Enum;
using OpenTK.Mathematics;

namespace MultiplayerVoxelGame.Game.Client.World.WorldGeneration.VoxelHandlers
{
    public class SurfaceHandler : VoxelHandler
    {
        private readonly int _waterLevel;
        private readonly VoxelType _surfaceBlockType;

        public SurfaceHandler(VoxelType surfaceBlockType, int waterLevel, VoxelHandler next)
            : base(next)
        {
            _surfaceBlockType = surfaceBlockType;
            _waterLevel = waterLevel;
        }

        protected override bool TryHandling(VoxelGenerationContext ctx)
        {
            if (ctx.DensityAbove <= 0 && ctx.Y > _waterLevel)
            {
                Vector3i pos = new Vector3i(ctx.X, ctx.Y, ctx.Z);
                ctx.ChunkData.SetVoxel(pos, _surfaceBlockType);
                return true;
            }

            return false;
        }
    }
}