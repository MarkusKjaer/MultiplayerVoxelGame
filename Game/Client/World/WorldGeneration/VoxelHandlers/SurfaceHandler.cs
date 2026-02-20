using CubeEngine.Engine.Client.World.Enum;

namespace MultiplayerVoxelGame.Game.Client.World.WorldGeneration.VoxelHandlers
{
    public class SurfaceHandler : VoxelHandler
    {
        protected override bool CanHandle(VoxelGenerationContext context)
            => context.Y == context.GroundHeight;

        protected override VoxelType Resolve(VoxelGenerationContext context)
            => context.GroundHeight < context.SeaLevel
                ? VoxelType.Stone
                : VoxelType.Grass;
    }
}
