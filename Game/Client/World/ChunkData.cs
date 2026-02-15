using OpenTK.Mathematics;

namespace CubeEngine.Engine.Client.World
{
    public struct ChunkData
    {
        public Voxel[] Voxels { get; private set; }
        public int SizeX { get; private set; }
        public int SizeY { get; private set; }
        public int SizeZ { get; private set; }

        public ChunkData(int sizeX, int sizeY, int sizeZ, Vector2 position)
        {
            SizeX = sizeX;
            SizeY = sizeY;
            SizeZ = sizeZ;
            Voxels = new Voxel[sizeX * sizeY * sizeZ];
            Position = position;
        }

        public int Index(int x, int y, int z)
        {
            if (x < 0 || x >= SizeX ||
                y < 0 || y >= SizeY ||
                z < 0 || z >= SizeZ)
                throw new ArgumentOutOfRangeException();

            return x + SizeX * (y + SizeY * z);
        }
        public Voxel GetVoxel(int x, int y, int z)
            => Voxels[Index(x, y, z)];
        public void SetVoxel(int x, int y, int z, Voxel voxel)
            => Voxels[Index(x, y, z)] = voxel;
        public Vector2 Position { get; private set; }
    }
}
