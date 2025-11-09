using System;
using System.Collections.Generic;
using System.Linq;

namespace CubeEngine.Engine.Network
{
    public class PlayerInputPacket : Packet
    {
        public ushort ClientId { get; set; }
        public List<PlayerInput> Inputs { get; } = new();

        public PlayerInputPacket(byte[] buffer) : base(PacketType.PlayerInput)
        {
            int index = 2;

            ClientId = BitConverter.ToUInt16(buffer, index);
            index += 2;

            ushort count = BitConverter.ToUInt16(buffer, index);
            index += 2;

            for (int i = 0; i < count; i++)
            {
                Inputs.Add((PlayerInput)buffer[index]);
                index++;
            }
        }

        public PlayerInputPacket(ushort clientId, IEnumerable<PlayerInput> inputs)
            : base(PacketType.PlayerInput)
        {
            ClientId = clientId;
            Inputs.AddRange(inputs);
        }

        private protected override byte[] SerializePayload()
        {
            var blocks = new List<byte[]>();

            blocks.Add(BitConverter.GetBytes(ClientId));

            blocks.Add(BitConverter.GetBytes((ushort)Inputs.Count));

            blocks.Add(Inputs.Select(i => (byte)i).ToArray());

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

    public enum PlayerInput : byte
    {
        MoveForward,
        MoveBackward,
        MoveLeft,
        MoveRight,
        Jump,
        Mine,
        Place,
    }
}