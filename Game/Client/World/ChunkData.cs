using CubeEngine.Engine.Client.World.Enum;
using OpenTK.Mathematics;

namespace CubeEngine.Engine.Client.World
{
    public class ChunkData
    {
        public byte[] Voxels { get; private set; }
        public int SizeX { get; private set; }
        public int SizeY { get; private set; }
        public int SizeZ { get; private set; }

        public bool IsDirty { get; set; }

        public Vector2 Position { get; private set; }

        public ChunkData(int sizeX, int sizeY, int sizeZ, Vector2 position)
        {
            SizeX = sizeX;
            SizeY = sizeY;
            SizeZ = sizeZ;
            Voxels = new byte[sizeX * sizeY * sizeZ];
            Position = position;
        }

        public int Index(int x, int y, int z)
        {
            if (x < 0 || x >= SizeX ||
                y < 0 || y >= SizeY ||
                z < 0 || z >= SizeZ)
            {
                throw new ArgumentOutOfRangeException(
                    $"Chunk Index out of range: ({x},{y},{z}) | " +
                    $"Chunk Size: ({SizeX},{SizeY},{SizeZ}) | " +
                    $"Chunk Position: {Position}");
            }

            return x + SizeX * (y + SizeY * z);
        }

        public VoxelType GetVoxel(int x, int y, int z)
        {
            if (x < 0 || x >= SizeX ||
                y < 0 || y >= SizeY ||
                z < 0 || z >= SizeZ)
            {
                return VoxelType.Empty;
            }

            return (VoxelType)Voxels[x + SizeX * (y + SizeY * z)];
        }
        public void SetVoxel(int x, int y, int z, VoxelType voxel)
        {
            if (x < 0 || x >= SizeX ||
                y < 0 || y >= SizeY ||
                z < 0 || z >= SizeZ)
                return;

            Voxels[x + SizeX * (y + SizeY * z)] = (byte)voxel;
            IsDirty = true;
        }

        public void SetVoxel(Vector3i pos, VoxelType voxel)
        {
            SetVoxel(pos.X, pos.Y, pos.Z, voxel);
        }
    }
}
