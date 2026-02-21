using CubeEngine.Engine.Client.World;
using CubeEngine.Engine.Client.World.Enum;
using OpenTK.Mathematics;

namespace MultiplayerVoxelGame.Game.Client.World.WorldGeneration.VoxelHandlers
{
    public abstract class VoxelHandler
    {
        private VoxelHandler _next;

        public VoxelHandler(VoxelHandler next)
        {
            _next = next;
        }

        public bool Handle(VoxelGenerationContext voxelGenerationContext)
        {
            if (TryHandling(voxelGenerationContext))
                return true;
            if (_next != null)
                return _next.Handle(voxelGenerationContext);
            return false;
        }

        protected abstract bool TryHandling(VoxelGenerationContext voxelGenerationContext);
    }
}
