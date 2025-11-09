namespace CubeEngine.Engine.Network
{
    public enum PacketType
    {
        Ping = 1,
        Connect,
        Disconnect,
        PlayerInfo,
        PlayerInput,
        PlayerState,
        ChunkInfo,
    }
}
