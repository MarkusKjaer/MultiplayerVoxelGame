using CubeEngine.Engine.Client.World.Enum;

namespace MultiplayerVoxelGame.Game.Client.World.WorldGeneration.VoxelHandlers
{
    public class UndergroundHandler : VoxelHandler
    {
        protected override bool CanHandle(VoxelGenerationContext context)
            => context.Y < context.GroundHeight;

        protected override VoxelType Resolve(VoxelGenerationContext context)
            => VoxelType.Stone;
    }
}
