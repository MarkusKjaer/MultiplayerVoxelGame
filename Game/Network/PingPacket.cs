namespace CubeEngine.Engine.Network
{
    internal class PingPacket : Packet
    {
        public PingPacket() : base(PacketType.Ping)
        {
            
        }
    }
}