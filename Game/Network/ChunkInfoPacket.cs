using CubeEngine.Engine.Client.World;
using CubeEngine.Engine.Client.World.Enum;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CubeEngine.Engine.Network
{
    public class ChunkInfoPacket : Packet
    {
        public Vector2 ChunkPos { get; set; }
        public ChunkData ChunkData { get; set; }
        public int ChunkSize { get; set; }
        public int MapHeight { get; set; }

        public ChunkInfoPacket(byte[] buffer) : base(PacketType.ChunkInfo)
        {
            int index = 2;

            ChunkPos = new Vector2(
                BitConverter.ToInt32(buffer, index),
                BitConverter.ToInt32(buffer, index + 4)
            );
            index += 8;

            ChunkSize = BitConverter.ToInt32(buffer, index);
            index += 4;
            MapHeight = BitConverter.ToInt32(buffer, index);
            index += 4;

            var chunkData = ChunkData;

            var position = new Vector2(
                BitConverter.ToSingle(buffer, index),
                BitConverter.ToSingle(buffer, index + 4)
            );
            index += 8;
            ChunkData = chunkData;


            int compressedLength = BitConverter.ToInt32(buffer, index);
            index += 4;

            chunkData = ChunkData;

            ChunkData = new(ChunkSize, MapHeight, ChunkSize, position);

            int voxelIndex = 0;
            while (voxelIndex < ChunkSize * MapHeight * ChunkSize)
            {
                byte type = buffer[index++];
                byte count = buffer[index++];
                for (int i = 0; i < count; i++)
                {
                    int x = voxelIndex / (MapHeight * ChunkSize);
                    int y = (voxelIndex / ChunkSize) % MapHeight;
                    int z = voxelIndex % ChunkSize;

                    ChunkData.SetVoxel(x, y, z, new Voxel
                    {
                        VoxelType = (VoxelType)type
                    });

                    voxelIndex++;
                }
            }
        }

        public ChunkInfoPacket(ChunkData chunkData, int chunkSize, int mapHeight)
            : base(PacketType.ChunkInfo)
        {
            ChunkData = chunkData;
            ChunkSize = chunkSize;
            MapHeight = mapHeight;
        }

        private protected override byte[] SerializePayload()
        {
            var blocks = new List<byte[]>();

            blocks.Add(BitConverter.GetBytes((int)ChunkPos.X));
            blocks.Add(BitConverter.GetBytes((int)ChunkPos.Y));

            blocks.Add(BitConverter.GetBytes(ChunkSize));
            blocks.Add(BitConverter.GetBytes(MapHeight));

            blocks.Add(BitConverter.GetBytes(ChunkData.Position.X));
            blocks.Add(BitConverter.GetBytes(ChunkData.Position.Y));

            var voxelBytes = new List<byte>();
            VoxelType? lastType = null;
            byte count = 0;

            for (int x = 0; x < ChunkSize; x++)
            {
                for (int y = 0; y < MapHeight; y++)
                {
                    for (int z = 0; z < ChunkSize; z++)
                    {
                        var current = ChunkData.GetVoxel(x, y, z).VoxelType;

                        if (lastType == null)
                        {
                            lastType = current; 
                            count = 1;
                        }
                        else if (lastType == current && count < 255)
                        {
                            count++;
                        }
                        else
                        {
                            voxelBytes.Add((byte)lastType.Value);
                            voxelBytes.Add(count);
                            lastType = current;
                            count = 1;
                        }
                    }
                }
            }

            if (lastType != null)
            {
                voxelBytes.Add((byte)lastType.Value);
                voxelBytes.Add(count);
            }

            blocks.Add(BitConverter.GetBytes(voxelBytes.Count));
            blocks.Add(voxelBytes.ToArray());

            int total = blocks.Sum(b => b.Length);
            var buffer = new byte[total];
            int pointer = 0;
            foreach (var b in blocks)
            {
                Buffer.BlockCopy(b, 0, buffer, pointer, b.Length);
                pointer += b.Length;
            }

            return buffer;
        }
    }
}
