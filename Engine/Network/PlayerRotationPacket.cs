namespace CubeEngine.Engine.Network
{
    public class PlayerRotationPacket : Packet
    {
        public ushort ClientId { get; set; }
        public float RotateX { get; set; } 
        public float RotateY { get; set; } 

        public PlayerRotationPacket(ushort clientId, float rotateX, float rotateY)
            : base(PacketType.PlayerRotation)
        {
            ClientId = clientId;
            RotateX = rotateX;
            RotateY = rotateY;
        }

        public PlayerRotationPacket(byte[] buffer) : base(PacketType.PlayerRotation)
        {
            int index = 2; 

            ClientId = BitConverter.ToUInt16(buffer, index);
            index += 2;

            RotateX = BitConverter.ToSingle(buffer, index);
            index += 4;

            RotateY = BitConverter.ToSingle(buffer, index);
        }

        private protected override byte[] SerializePayload()
        {
            var buffer = new List<byte>();
            buffer.AddRange(BitConverter.GetBytes(ClientId));
            buffer.AddRange(BitConverter.GetBytes(RotateX));
            buffer.AddRange(BitConverter.GetBytes(RotateY));
            return buffer.ToArray();
        }
    }

}
