using OpenTK.Mathematics;

namespace CubeEngine.Engine.Client.World
{
    public struct ChunkData
    {
        public Voxel[,,] Voxels { get; set; }
        public Vector3 Position { get; set; }
    }
}
