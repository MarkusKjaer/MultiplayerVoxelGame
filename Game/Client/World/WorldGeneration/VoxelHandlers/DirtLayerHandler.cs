using CubeEngine.Engine.Client.World.Enum;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiplayerVoxelGame.Game.Client.World.WorldGeneration.VoxelHandlers
{
    public class DirtLayerHandler : VoxelHandler
    {
        private float _stoneDensityThreshold;

        public DirtLayerHandler(VoxelHandler next, float stoneDensityThreshold = 3.0f) : base(next)
        {
            _stoneDensityThreshold = stoneDensityThreshold;
        }

        protected override bool TryHandling(VoxelGenerationContext ctx)
        {
            Vector3i pos = new Vector3i(ctx.X, ctx.Y, ctx.Z);

            if (ctx.Density < _stoneDensityThreshold)
            {
                ctx.ChunkData.SetVoxel(pos, VoxelType.Dirt);
                return true;
            }

            return false;
        }
    }
}
