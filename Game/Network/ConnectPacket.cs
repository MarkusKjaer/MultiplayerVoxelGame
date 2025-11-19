using System.Text;

namespace CubeEngine.Engine.Network
{
    public class ConnectPacket : Packet
    {
        public string PlayerName { get; set; } = string.Empty;

        public ConnectPacket(byte[] buffer) : base(PacketType.Connect)
        {
            int index = 2;

            ushort nameLength = BitConverter.ToUInt16(buffer, index);
            index += 2;

            PlayerName = Encoding.UTF8.GetString(buffer, index, nameLength);
        }

        public ConnectPacket(string playerName) : base(PacketType.Connect)
        {
            PlayerName = playerName;
        }

        private protected override byte[] SerializePayload()
        {
            byte[] nameBytes = Encoding.UTF8.GetBytes(PlayerName);
            ushort nameLength = (ushort)nameBytes.Length;

            var blocks = new List<byte[]>
            {
                BitConverter.GetBytes(nameLength),
                nameBytes
            };

            int totalLength = blocks.Sum(b => b.Length);
            var buffer = new byte[totalLength];
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
