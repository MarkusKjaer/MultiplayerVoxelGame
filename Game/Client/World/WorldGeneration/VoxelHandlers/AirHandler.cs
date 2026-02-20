using CubeEngine.Engine.Client.World.Enum;

namespace MultiplayerVoxelGame.Game.Client.World.WorldGeneration.VoxelHandlers
{
    public class AirHandler : VoxelHandler
    {
        protected override bool CanHandle(VoxelGenerationContext context) => true;

        protected override VoxelType Resolve(VoxelGenerationContext context)
            => VoxelType.Empty;
    }
}
