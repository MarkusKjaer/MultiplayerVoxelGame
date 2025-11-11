using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CubeEngine.Engine.Network
{
    public class PlayerStatePacket : Packet
    {
        public ushort ClientId { get; private set; }
        public Vector3 Position { get; private set; }
        public Quaternion Orientation { get; private set; }
        public Quaternion HeadOrientation { get; private set; }

        public PlayerStatePacket(byte[] buffer) : base(PacketType.PlayerState)
        {
            int index = 2; 

            ClientId = BitConverter.ToUInt16(buffer, index);
            index += 2;

            float px = BitConverter.ToSingle(buffer, index); index += 4;
            float py = BitConverter.ToSingle(buffer, index); index += 4;
            float pz = BitConverter.ToSingle(buffer, index); index += 4;
            Position = new Vector3(px, py, pz);

            float ox = BitConverter.ToSingle(buffer, index); index += 4;
            float oy = BitConverter.ToSingle(buffer, index); index += 4;
            float oz = BitConverter.ToSingle(buffer, index); index += 4;
            float ow = BitConverter.ToSingle(buffer, index); index += 4;
            Orientation = new Quaternion(ox, oy, oz, ow);

            float hx = BitConverter.ToSingle(buffer, index); index += 4;
            float hy = BitConverter.ToSingle(buffer, index); index += 4;
            float hz = BitConverter.ToSingle(buffer, index); index += 4;
            float hw = BitConverter.ToSingle(buffer, index); index += 4;
            HeadOrientation = new Quaternion(hx, hy, hz, hw);
        }

        public PlayerStatePacket(
            ushort clientId,
            Vector3 position,
            Quaternion orientation,
            Quaternion headOrientation)
            : base(PacketType.PlayerState)
        {
            ClientId = clientId;
            Position = position;
            Orientation = orientation;
            HeadOrientation = headOrientation;
        }

        private protected override byte[] SerializePayload()
        {
            var blocks = new List<byte[]>();

            blocks.Add(BitConverter.GetBytes(ClientId));

            blocks.Add(BitConverter.GetBytes(Position.X));
            blocks.Add(BitConverter.GetBytes(Position.Y));
            blocks.Add(BitConverter.GetBytes(Position.Z));

            blocks.Add(BitConverter.GetBytes(Orientation.X));
            blocks.Add(BitConverter.GetBytes(Orientation.Y));
            blocks.Add(BitConverter.GetBytes(Orientation.Z));
            blocks.Add(BitConverter.GetBytes(Orientation.W));

            blocks.Add(BitConverter.GetBytes(HeadOrientation.X));
            blocks.Add(BitConverter.GetBytes(HeadOrientation.Y));
            blocks.Add(BitConverter.GetBytes(HeadOrientation.Z));
            blocks.Add(BitConverter.GetBytes(HeadOrientation.W));

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
