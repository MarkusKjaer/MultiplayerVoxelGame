using CubeEngine.Engine.Client.World;
using CubeEngine.Engine.Client.World.Enum;
using OpenTK.Mathematics;

namespace MultiplayerVoxelGame.Game.Client.World.WorldGeneration.VoxelHandlers
{
    public class StoneLayerHandler : VoxelHandler
    {
        private float _stoneThreshold;

        private NoiseSettings _stoneNoiseSettings;

        private DomainWarping _domainWarping;

        public StoneLayerHandler(NoiseSettings stoneNoiseSettings, DomainWarping domainWarping, VoxelHandler next, float stoneThreshold = 0.5f) : base(next)
        {
            _stoneNoiseSettings = stoneNoiseSettings;
            _domainWarping = domainWarping;
            _stoneThreshold = stoneThreshold;
        }

        protected override bool TryHandling(VoxelGenerationContext ctx)
        {
            if (ctx.Y > ctx.SurfaceHeightNoise) return false;

            int mapIndex = ctx.Z * ctx.ChunkData.SizeX + ctx.X;
            float stoneNoise = 0.5f;

            if (stoneNoise > _stoneThreshold)
            {
                ctx.ChunkData.SetVoxel(new Vector3i(ctx.X, ctx.Y, ctx.Z), VoxelType.Stone);
                return true;
            }
            return false;
        }
    }
}
