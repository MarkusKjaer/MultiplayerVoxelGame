using CubeEngine.Engine.Network;
using OpenTK.Mathematics;
using System.Net;
using System.Net.Sockets;

namespace CubeEngine.Engine.Server
{
    public class ClientInstance
    {
        public Guid Id { get; } = Guid.NewGuid(); 
        public TcpClient TcpClient { get; }
        public IPEndPoint TcpEndPoint { get; }
        public DateTime ConnectedAt { get; } = DateTime.Now;

        public string Username { get; set; } = "Unknown"; 

        public Vector3 Postion { get; set; }
        public Quaternion Orientation { get; set; }

        public ClientInstance(TcpClient client)
        {
            TcpClient = client;
            TcpEndPoint = (IPEndPoint)client.Client.RemoteEndPoint!;

            Setup();
        }

        private void Setup()
        {
            //ServerMap serverMap = GameServer.Instance.ServerMap;
            //List<ServerChunk> serverChunks = serverMap.CurrentChunks;
            //int chunkSize = serverMap.ChunkSize;
            //int maxWorldHeight = serverMap.MaxWorldHeight;

            //foreach (ServerChunk chunk in serverChunks) 
            //{
            //    Packet packet = new ChunkInfoPacket(chunk.ChunkData, chunkSize, maxWorldHeight);
            //    _ = GameServer.Instance.SendTcpPacket(TcpClient, packet);
            //}
        }

        public override string ToString()
        {
            return $"{Id} | {TcpEndPoint}";
        }
    }
}
