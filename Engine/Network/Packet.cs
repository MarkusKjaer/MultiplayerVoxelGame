using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CubeEngine.Engine.Network
{
    public abstract class Packet(PacketType type)
    {
        public PacketType Type => type;

        public byte[] Serialize()
        {
            var typeBlock = BitConverter.GetBytes((ushort)type);
            var blocks = new List<byte[]> { typeBlock, SerializePayload() };
            var packetBuffer = new byte[blocks.Sum(b => b.Length)];
            var bufPointer = 0;
            try
            {
                blocks.ForEach(b =>
                {
                    Buffer.BlockCopy(b, 0, packetBuffer, bufPointer, b.Length);
                    bufPointer += b.Length;
                });
            }
            catch (Exception e)
            {
                Console.Error.WriteLine($"Failed to serialize packet. Packet has now been invalidated: {e}");
                return Array.Empty<byte>();
            }

            return packetBuffer;
        }

        public static Packet Deserialize(byte[] buffer)
        {
            PacketType packetType = (PacketType)BitConverter.ToUInt16(buffer, 0);
            switch (packetType)
            {
                case PacketType.Connect:
                    return new ConnectPacket(buffer);
                case PacketType.PlayerState:
                    return new PlayerStatePacket(buffer);
                case PacketType.PlayerInput:
                    return new PlayerInputPacket(buffer);
                default:
                    throw new Exception($"Unknown packet type: {packetType}");
            }
        }

        private protected virtual byte[] SerializePayload()
        {
            return [];
        }
    }
}
