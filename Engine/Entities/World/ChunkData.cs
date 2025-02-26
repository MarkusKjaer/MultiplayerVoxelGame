using OpenTK.Mathematics;

namespace CubeEngine.Engine.Entities.World
{
    public struct ChunkData
    {
        public Voxel[,,] Voxels;
        public Vector3 Position;
    }
}
