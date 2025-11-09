using CubeEngine.Engine.Client.World;
using CubeEngine.Engine.Network;
using OpenTK.Mathematics;
using System.Net;

namespace CubeEngine.Engine.Server
{
    public class ServerMap
    {
        public Dictionary<Vector2, ServerChunk> CurrentChunks = [];

        private WorldGen _worldGen;

        public int ChunkSize { get; private set; }
        public int MaxWorldHeight { get; private set; }

        public ServerMap(int chunkSize, int maxWorldHeight, int seed)
        {
            ChunkSize = chunkSize;
            MaxWorldHeight = maxWorldHeight;

            List<Vector2> chunksToGen =
            [
                new(0, 0), new(1, 0), new(2, 0), new(3, 0), new(4, 0),
                new(0, 1), new(1, 1), new(2, 1), new(3, 1), new(4, 1),
                new(0, 2), new(1, 2), new(2, 2), new(3, 2), new(4, 2),
                new(0, 3), new(1, 3), new(2, 3), new(3, 3), new(4, 3),
                new(0, 4), new(1, 4), new(2, 4), new(3, 4), new(4, 4),
            ];

            if (seed == 0)
            {
                _worldGen = new WorldGen();
            }
            else
            {
                _worldGen = new WorldGen(seed);
            }

            var newChunks = _worldGen.GenPartOfWorld(chunkSize, maxWorldHeight, chunksToGen);

            for (int i = 0; i < newChunks.Count; i++)
            {
                CurrentChunks.Add(newChunks[i].Position / ChunkSize, new ServerChunk(newChunks[i]));
            }

            GameServer.Instance.OnClientMessage += OnClientMessage;
        }

        private void OnClientMessage(IPEndPoint sender, Packet packet)
        {
            if (!GameServer.Instance.ClientsByEndpoint.TryGetValue(sender, out var client))
                return;  // Unknown client

            switch (packet)
            {
                case ChunkRequestPacket req:
                    ChunkData chunk = CurrentChunks[req.ChunkPos].ChunkData;

                    ChunkInfoPacket response = new(chunk, ChunkSize, MaxWorldHeight);
                    GameServer.Instance.SendTcpPacket(client.TcpClient, response);

                    break;
            }
        }

    }
}
