using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CubeEngine.Engine.Network
{
    public class PlayerStatePacket : Packet
    {
        public ushort ClientId;
        public Vector3 Position;
        private Quaternion Orientation;

        // Deserialize constructor
        public PlayerStatePacket(byte[] buffer) : base(PacketType.PlayerState)
        {
            int index = 2; // skip packet type

            ClientId = BitConverter.ToUInt16(buffer, index);
            index += 2;

            // Position (3 floats)
            float px = BitConverter.ToSingle(buffer, index); index += 4;
            float py = BitConverter.ToSingle(buffer, index); index += 4;
            float pz = BitConverter.ToSingle(buffer, index); index += 4;
            Position = new Vector3(px, py, pz);

            // Orientation (4 floats)
            float ox = BitConverter.ToSingle(buffer, index); index += 4;
            float oy = BitConverter.ToSingle(buffer, index); index += 4;
            float oz = BitConverter.ToSingle(buffer, index); index += 4;
            float ow = BitConverter.ToSingle(buffer, index); index += 4;
            Orientation = new Quaternion(ox, oy, oz, ow);
        }

        public PlayerStatePacket(ushort clientId, Vector3 position, Quaternion orientation)
            : base(PacketType.PlayerState)
        {
            ClientId = clientId;
            Position = position;
            Orientation = orientation;
        }

        private protected override byte[] SerializePayload()
        {
            var blocks = new List<byte[]>();

            // Client ID
            blocks.Add(BitConverter.GetBytes(ClientId));

            // Position
            blocks.Add(BitConverter.GetBytes(Position.X));
            blocks.Add(BitConverter.GetBytes(Position.Y));
            blocks.Add(BitConverter.GetBytes(Position.Z));

            // Orientation
            blocks.Add(BitConverter.GetBytes(Orientation.X));
            blocks.Add(BitConverter.GetBytes(Orientation.Y));
            blocks.Add(BitConverter.GetBytes(Orientation.Z));
            blocks.Add(BitConverter.GetBytes(Orientation.W));

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
