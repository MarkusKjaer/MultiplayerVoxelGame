using CubeEngine.Engine.Client.World.Enum;

namespace MultiplayerVoxelGame.Game.Client.World.WorldGeneration.VoxelHandlers
{
    public abstract class VoxelHandler
    {
        protected VoxelHandler? Next;

        public VoxelHandler SetNext(VoxelHandler next)
        {
            Next = next;
            return next;
        }

        public VoxelType Handle(VoxelGenerationContext context)
        {
            if (CanHandle(context))
                return Resolve(context);

            if (Next != null)
                return Next.Handle(context);

            return VoxelType.Empty;
        }

        protected abstract bool CanHandle(VoxelGenerationContext context);
        protected abstract VoxelType Resolve(VoxelGenerationContext context);
    }
}
