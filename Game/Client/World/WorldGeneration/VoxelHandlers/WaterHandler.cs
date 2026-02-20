using CubeEngine.Engine.Client.World.Enum;

namespace MultiplayerVoxelGame.Game.Client.World.WorldGeneration.VoxelHandlers
{
    public class WaterHandler : VoxelHandler
    {
        protected override bool CanHandle(VoxelGenerationContext context)
            => context.Y <= context.SeaLevel;

        protected override VoxelType Resolve(VoxelGenerationContext context)
            => VoxelType.Water;
    }
}
