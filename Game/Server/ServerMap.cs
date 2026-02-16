using CubeEngine.Engine.Client.World;
using CubeEngine.Engine.Client.World.Enum;
using CubeEngine.Engine.Network;
using OpenTK.Mathematics;
using System.Collections.Concurrent;
using System.Net;

namespace CubeEngine.Engine.Server
{
    public class ServerMap
    {
        public class ChunkInfo
        {
            public required ServerChunk ServerChunk;
            public required Object Lock;
        }

        public ConcurrentDictionary<Vector2, ChunkInfo> CurrentChunks = new();

        private WorldGen _worldGen;

        public int ChunkSize { get; private set; }
        public int MaxWorldHeight { get; private set; }

        public ServerMap(int chunkSize, int maxWorldHeight, int seed)
        {
            ChunkSize = chunkSize;
            MaxWorldHeight = maxWorldHeight;

            //List<Vector2> chunksToGen =
            //[
            //    new(0, 0), new(1, 0), new(2, 0), new(3, 0), new(4, 0),
            //    new(0, 1), new(1, 1), new(2, 1), new(3, 1), new(4, 1),
            //    new(0, 2), new(1, 2), new(2, 2), new(3, 2), new(4, 2),
            //    new(0, 3), new(1, 3), new(2, 3), new(3, 3), new(4, 3),
            //    new(0, 4), new(1, 4), new(2, 4), new(3, 4), new(4, 4),
            //];

            if (seed == 0)
            {
                _worldGen = new WorldGen();
            }
            else
            {
                _worldGen = new WorldGen(seed);
            }

            //var newChunks = _worldGen.GenPartOfWorld(chunkSize, maxWorldHeight, chunksToGen);

            //for (int i = 0; i < newChunks.Count; i++)
            //{
            //    CurrentChunks.Add(newChunks[i].Position / ChunkSize, new ServerChunk(newChunks[i]));
            //}

            GameServer.Instance.ClientMessage += OnClientMessage;
        }

        private void OnClientMessage(IPEndPoint sender, Packet packet)
        {
            if (!GameServer.Instance.ClientsByEndpoint.TryGetValue(sender, out var client))
                return;  // Unknown client

            switch (packet)
            {
                case ChunkRequestPacket req:
                    Vector2 chunkPos = req.ChunkPos;
                    SendBackChunk(client, chunkPos);
                    break;  
            }
        }

        private void SendBackChunk(ClientInstance client, Vector2 chunkPos)
        {
            var chunkInfo = CurrentChunks.GetOrAdd(chunkPos, pos =>
            {
                return new ChunkInfo
                {
                    ServerChunk = null!, 
                    Lock = new object()
                };
            });

            if (chunkInfo == null)
                return;

            lock (chunkInfo.Lock)
            {
                if (chunkInfo.ServerChunk == null)
                {
                    var chunksToGen = new List<Vector2> { chunkPos };
                    var newChunks = _worldGen.GenPartOfWorld(ChunkSize, MaxWorldHeight, chunksToGen);

                    if (newChunks.Count == 0)
                        throw new Exception("Chunk generation failed");

                    chunkInfo.ServerChunk = new ServerChunk(newChunks[0]);

                    Console.WriteLine($"Generated new chunk at {chunkPos.X}, {chunkPos.Y}");
                }
            }

            try
            {
                ChunkInfoPacket response = new(chunkInfo.ServerChunk.ChunkData, ChunkSize, MaxWorldHeight);
                _ = GameServer.Instance.SendTcpPacket(client.TcpClient, response);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void SetBlock(int x, int y, int z, VoxelType type)
        {
            if (!TryGetChunkFromWorldPos(x, z, out var chunk, out int cx, out int cz))
                return;

            if (y < 0 || y >= MaxWorldHeight)
                return;

            chunk.ChunkData.SetVoxel(cx, y, cz, new Voxel { VoxelType = type });
        }

        public void BroadcastChunkUpdate(Vector3i blockPos)
        {
            if (!TryGetChunkFromWorldPos(blockPos.X, blockPos.Z, out var chunk, out _, out _))
                return;

            var packet = new ChunkInfoPacket(chunk.ChunkData, ChunkSize, MaxWorldHeight);

            foreach (var client in GameServer.Instance.ClientInstances.Values)
            {
                _ = GameServer.Instance.SendTcpPacket(client.TcpClient, packet);
            }
        }

        private bool TryGetChunkFromWorldPos(int x, int z, out ServerChunk chunk, out int cx, out int cz)
        {
            cx = 0;
            cz = 0;
            chunk = null;

            int chunkX = (int)MathF.Floor(x / (float)ChunkSize);
            int chunkZ = (int)MathF.Floor(z / (float)ChunkSize);

            Vector2 chunkKey = new(chunkX, chunkZ);

            if (!CurrentChunks.TryGetValue(chunkKey, out ChunkInfo chunkInfo))
                return false;

            chunk = chunkInfo.ServerChunk;

            cx = Mod(x, ChunkSize);
            cz = Mod(z, ChunkSize);

            return true;
        }

        private int Mod(int value, int m)
        {
            int r = value % m;
            return r < 0 ? r + m : r;
        }
    }
}
