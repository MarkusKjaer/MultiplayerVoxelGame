namespace CubeEngine.Engine.Network
{
    public class PlayerRotationPacket : Packet
    {
        public float RotateX { get; set; } 
        public float RotateY { get; set; } 

        public PlayerRotationPacket(float rotateX, float rotateY)
            : base(PacketType.PlayerRotation)
        {
            RotateX = rotateX;
            RotateY = rotateY;
        }

        public PlayerRotationPacket(byte[] buffer) : base(PacketType.PlayerRotation)
        {
            int index = 2; 

            RotateX = BitConverter.ToSingle(buffer, index);
            index += 4;

            RotateY = BitConverter.ToSingle(buffer, index);
        }

        private protected override byte[] SerializePayload()
        {
            var buffer = new List<byte>();
            buffer.AddRange(BitConverter.GetBytes(RotateX));
            buffer.AddRange(BitConverter.GetBytes(RotateY));
            return buffer.ToArray();
        }
    }

}
