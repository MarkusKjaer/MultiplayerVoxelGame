using System.Text;

namespace CubeEngine.Engine.Network
{
    public class PlayerJoinedPacket : Packet
    {
        public int PlayerId { get; set; }
        public string PlayerName { get; set; } = string.Empty;

        public PlayerJoinedPacket(byte[] buffer) : base(PacketType.PlayerJoined)
        {
            int index = 2; 

            PlayerId = BitConverter.ToInt32(buffer, index);
            index += 4;

            ushort nameLength = BitConverter.ToUInt16(buffer, index);
            index += 2;

            PlayerName = Encoding.UTF8.GetString(buffer, index, nameLength);
        }

        public PlayerJoinedPacket(int playerId, string playerName) : base(PacketType.PlayerJoined)
        {
            PlayerId = playerId;
            PlayerName = playerName;
        }

        private protected override byte[] SerializePayload()
        {
            byte[] nameBytes = Encoding.UTF8.GetBytes(PlayerName);
            ushort nameLength = (ushort)nameBytes.Length;

            var blocks = new List<byte[]>
            {
                BitConverter.GetBytes(PlayerId),
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
