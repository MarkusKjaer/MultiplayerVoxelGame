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

        public VoxelType GetVoxel(int x, int y, int z)
        {
            if (x >= 0 && x < ChunkData.SizeX &&
                y >= 0 && y < ChunkData.SizeY &&
                z >= 0 && z < ChunkData.SizeZ)
            {
                return ChunkData.GetVoxel(x, y, z);
            }
            else
            {
                return VoxelType.Empty;
            }
        }
    }
}
