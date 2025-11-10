namespace CubeEngine.Engine.Network
{
    public enum PacketType
    {
        Ping = 1,
        Connect,
        Disconnect,
        PlayerJoined,
        PlayerInput,
        PlayerState,
        ChunkInfo,
        ChunkRequest,
        PlayerRotation,
    }
}
