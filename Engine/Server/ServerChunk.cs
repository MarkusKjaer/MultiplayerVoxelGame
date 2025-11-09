using CubeEngine.Engine.Client.World;
using CubeEngine.Engine.Client.World.Enum;

namespace CubeEngine.Engine.Server
{
    public class ServerChunk
    {
        private ChunkData _chunkData;

        public ChunkData ChunkData
        {
            get
            {
                return _chunkData;
            }
            set
            {
                _chunkData = value;
            }
        }

        public ServerChunk(ChunkData chunkData)
        {
            _chunkData = chunkData;
        }

        public Voxel GetVoxel(int x, int y, int z)
        {
            if (x >= 0 && x < ChunkData.Voxels.GetLength(0) &&
                y >= 0 && y < ChunkData.Voxels.GetLength(1) &&
                z >= 0 && z < ChunkData.Voxels.GetLength(2))
            {
                return ChunkData.Voxels[x, y, z];
            }
            else
            {
                Voxel v = new Voxel();
                v.VoxelType = VoxelType.Empty;
                return v;
            }
        }
    }
}
