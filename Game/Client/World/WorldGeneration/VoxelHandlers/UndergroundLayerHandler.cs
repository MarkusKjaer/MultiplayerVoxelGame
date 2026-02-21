using CubeEngine.Engine.Client.World.Enum;
using OpenTK.Mathematics;

namespace MultiplayerVoxelGame.Game.Client.World.WorldGeneration.VoxelHandlers
{
    public class UndergroundLayerHandler : VoxelHandler
    {
        private VoxelType _undergroundBlockType;

        public UndergroundLayerHandler(VoxelHandler next, VoxelType undergroundBlockType) : base(next)
        {
            _undergroundBlockType = undergroundBlockType;
        }

        protected override bool TryHandling(VoxelGenerationContext ctx)
        {
            Vector3i pos = new Vector3i(ctx.X, ctx.Y, ctx.Z);
            ctx.ChunkData.SetVoxel(pos, _undergroundBlockType); 
            return true;
        }
    }
}
