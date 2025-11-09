using OpenTK.Mathematics;

namespace CubeEngine.Engine.Network
{
    public class ChunkRequestPacket : Packet
    {
        public Vector2 ChunkPos { get; private set; }

        public ChunkRequestPacket(byte[] buffer) : base(PacketType.ChunkRequest)
        {
            int index = 2; 

            float x = BitConverter.ToSingle(buffer, index);
            index += 4;

            float y = BitConverter.ToSingle(buffer, index);
            index += 4;

            ChunkPos = new Vector2(x, y);
        }

        public ChunkRequestPacket(Vector2 chunkPos)
            : base(PacketType.ChunkRequest)
        {
            ChunkPos = chunkPos;
        }

        private protected override byte[] SerializePayload()
        {
            var blocks = new List<byte[]>();

            blocks.Add(BitConverter.GetBytes(ChunkPos.X));
            blocks.Add(BitConverter.GetBytes(ChunkPos.Y));

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
