namespace CubeEngine.Engine.Network
{
    public enum PacketType
    {
        Ping = 1,
        Connect,
        Disconnect,
        PlayerJoinConfirm,
        PlayerInput,
        PlayerState,
        ChunkInfo,
        ChunkRequest,
        PlayerRotation,
    }
}
