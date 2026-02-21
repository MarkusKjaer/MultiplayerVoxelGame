using CubeEngine.Engine.Client.World;
using CubeEngine.Engine.Client.World.Enum;
using CubeEngine.Engine.Network;
using MultiplayerVoxelGame.Game.Client.World.WorldGeneration;
using MultiplayerVoxelGame.Util.Settings;
using OpenTK.Mathematics;
using System.Collections.Concurrent;
using System.IO.Compression;
using System.Net;
using System.Threading.Channels;

namespace CubeEngine.Engine.Server
{
    public class ServerMap
    {
        public class ChunkInfo
        {
            public required ServerChunk ServerChunk;
            public required Object Lock;
        }

        private readonly struct ChunkSaveRequest
        {
            public readonly int ChunkX;
            public readonly int ChunkZ;
            public readonly byte[] CompressedData;

            public ChunkSaveRequest(int x, int z, byte[] data)
            {
                ChunkX = x;
                ChunkZ = z;
                CompressedData = data;
            }
        }

        private readonly Channel<ChunkSaveRequest> _saveQueue = Channel.CreateUnbounded<ChunkSaveRequest>();

        public ConcurrentDictionary<Vector2, ChunkInfo> CurrentChunks = new();
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _regionLocks = new();

        private WorldGeneration _worldGen;

        public int ChunkSize { get; private set; }
        public int MaxWorldHeight { get; private set; }

        private const int REGION_SIZE = 32;
        private const int SECTOR_BYTES = 4096;
        private const int HEADER_BYTES = SECTOR_BYTES * 2;

        private readonly string _worldPath = "world";

        public ServerMap(int chunkSize, int maxWorldHeight, int seed)
        {
            ChunkSize = chunkSize;
            MaxWorldHeight = maxWorldHeight;

            if (seed == 0)
            {
                _worldGen = new WorldGeneration();
            }
            else
            {
                _worldGen = new WorldGeneration(seed);
            }

            GameServer.Instance.ClientMessage += OnClientMessage;

            _ = Task.Run(ProcessSaveQueueAsync);
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
            int chunkX = (int)chunkPos.X;
            int chunkZ = (int)chunkPos.Y;

            var chunkInfo = CurrentChunks.GetOrAdd(chunkPos, _ => new ChunkInfo
            {
                ServerChunk = null!,
                Lock = new object() 
            });

            Task.Run(() =>
            {
                ServerChunk localChunkRef = null;

                lock (chunkInfo.Lock)
                {
                    if (chunkInfo.ServerChunk != null)
                    {
                        localChunkRef = chunkInfo.ServerChunk;
                    }
                }

                if (localChunkRef == null)
                {
                    localChunkRef = LoadChunk(chunkX, chunkZ);

                    if (localChunkRef != null)
                    {
                        Console.WriteLine($"Loaded chunk from disk at {chunkX},{chunkZ}");
                    }
                    else
                    {
                        var chunksToGen = new List<Vector2> { chunkPos };
                        var newChunks = _worldGen.GenPartOfWorld(ChunkSize, MaxWorldHeight, chunksToGen);
                        localChunkRef = new ServerChunk(newChunks[0]);

                        SaveChunk(localChunkRef);
                        Console.WriteLine($"Generated & saved chunk at {chunkX},{chunkZ}");
                    }

                    lock (chunkInfo.Lock)
                    {
                        if (chunkInfo.ServerChunk == null)
                        {
                            chunkInfo.ServerChunk = localChunkRef;
                        }
                        else
                        {
                            localChunkRef = chunkInfo.ServerChunk;
                        }
                    }
                }

                try
                {
                    var packet = new ChunkInfoPacket(localChunkRef.ChunkData, ChunkSize, MaxWorldHeight);
                    _ = GameServer.Instance.SendTcpPacket(client.TcpClient, packet);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Chunk send failed: {e}");
                }
            });
        }

        public void SetBlock(int x, int y, int z, VoxelType type)
        {
            if (!TryGetChunkFromWorldPos(x, z, out var chunk, out int cx, out int cz))
                return;

            if (y < 0 || y >= MaxWorldHeight)
                return;

            // Only update if actually changing
            var current = chunk.ChunkData.GetVoxel(cx, y, cz);
            if (current == type)
                return;

            chunk.ChunkData.SetVoxel(cx, y, cz, type);

            SaveChunk(chunk);

            BroadcastChunkUpdate(new Vector3i(x, y, z));
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

        #region Save & Load

        private (int regionX, int regionZ) GetRegionCoords(int chunkX, int chunkZ)
        {
            return (
                Math.DivRem(chunkX, REGION_SIZE, out _) >= 0
                    ? chunkX / REGION_SIZE
                    : ((chunkX + 1) / REGION_SIZE) - 1,

                Math.DivRem(chunkZ, REGION_SIZE, out _) >= 0
                    ? chunkZ / REGION_SIZE
                    : ((chunkZ + 1) / REGION_SIZE) - 1
            );
        }

        private int GetChunkIndexInRegion(int chunkX, int chunkZ)
        {
            int localX = Mod(chunkX, REGION_SIZE);
            int localZ = Mod(chunkZ, REGION_SIZE);
            return localX + localZ * REGION_SIZE;
        }

        private string GetRegionPath(int regionX, int regionZ)
        {
            return Path.Combine(_worldPath, "region", $"r.{regionX}.{regionZ}.mca");
        }

        private async Task ProcessSaveQueueAsync()
        {
            // This loop asynchronously waits for new items. 
            // It yields the thread when empty, using 0% CPU.
            await foreach (var request in _saveQueue.Reader.ReadAllAsync())
            {
                try
                {
                    await WriteChunkToRegionAsync(request.ChunkX, request.ChunkZ, request.CompressedData);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to save chunk {request.ChunkX},{request.ChunkZ}: {ex.Message}");
                }
            }
        }

        public void SaveChunk(ServerChunk chunk)
        {
            int chunkX = (int)(chunk.ChunkData.Position.X / ChunkSize);
            int chunkZ = (int)(chunk.ChunkData.Position.Y / ChunkSize);

            // CPU-bound work is fine here (it's already on a background thread from our last fix)
            byte[] rawData = SerializeChunk(chunk);
            byte[] compressed;

            using (var ms = new MemoryStream())
            {
                using (var z = new System.IO.Compression.ZLibStream(ms, CompressionLevel.Fastest))
                    z.Write(rawData);

                compressed = ms.ToArray();
            }

            // Toss it into the queue and immediately return!
            _saveQueue.Writer.TryWrite(new ChunkSaveRequest(chunkX, chunkZ, compressed));
        }

        private async Task WriteChunkToRegionAsync(int chunkX, int chunkZ, byte[] compressedData)
        {
            var (regionX, regionZ) = GetRegionCoords(chunkX, chunkZ);
            string path = GetRegionPath(regionX, regionZ);

            var regionLock = _regionLocks.GetOrAdd(path, _ => new SemaphoreSlim(1, 1));
            await regionLock.WaitAsync();

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);

                using var fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite, 4096, FileOptions.Asynchronous);

                if (fs.Length < HEADER_BYTES)
                    fs.SetLength(HEADER_BYTES);

                int index = GetChunkIndexInRegion(chunkX, chunkZ);
                int totalLength = compressedData.Length + 5;
                int sectorCount = (totalLength + SECTOR_BYTES - 1) / SECTOR_BYTES;

                long fileLength = fs.Length;
                long alignedLength = ((fileLength + SECTOR_BYTES - 1) / SECTOR_BYTES) * SECTOR_BYTES;

                if (fileLength != alignedLength)
                    fs.SetLength(alignedLength);

                int sectorOffset = (int)(alignedLength / SECTOR_BYTES);

                // Write chunk data into a memory buffer first
                using var chunkBuffer = new MemoryStream();
                using var bw = new BinaryWriter(chunkBuffer, System.Text.Encoding.Default, true);

                bw.Write(compressedData.Length + 1);
                bw.Write((byte)2);
                bw.Write(compressedData);

                int padding = sectorCount * SECTOR_BYTES - totalLength;
                if (padding > 0)
                    bw.Write(new byte[padding]);

                // Asynchronously write the data to disk
                fs.Seek(sectorOffset * SECTOR_BYTES, SeekOrigin.Begin);
                byte[] chunkBytes = chunkBuffer.ToArray();
                await fs.WriteAsync(chunkBytes, 0, chunkBytes.Length);

                // Update header in a memory buffer
                using var headerBuffer = new MemoryStream();
                using var headerBw = new BinaryWriter(headerBuffer);
                headerBw.Write((byte)(sectorOffset >> 16));
                headerBw.Write((byte)(sectorOffset >> 8));
                headerBw.Write((byte)(sectorOffset));
                headerBw.Write((byte)sectorCount);

                // Asynchronously write the header to disk
                fs.Seek(index * 4, SeekOrigin.Begin);
                byte[] headerBytes = headerBuffer.ToArray();
                await fs.WriteAsync(headerBytes, 0, headerBytes.Length);
            }
            finally
            {
                regionLock.Release();
            }
        }

        public ServerChunk? LoadChunk(int chunkX, int chunkZ)
        {
            var (regionX, regionZ) = GetRegionCoords(chunkX, chunkZ);
            string path = GetRegionPath(regionX, regionZ);

            if (!File.Exists(path)) return null;

            var regionLock = _regionLocks.GetOrAdd(path, _ => new SemaphoreSlim(1, 1));
            regionLock.Wait();

            try
            {
                using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var br = new BinaryReader(fs);

                int index = GetChunkIndexInRegion(chunkX, chunkZ);

                fs.Seek(index * 4, SeekOrigin.Begin);

                int offset = (br.ReadByte() << 16) |
                             (br.ReadByte() << 8) |
                             br.ReadByte();

                int sectorCount = br.ReadByte();

                if (offset == 0)
                    return null;

                fs.Seek(offset * SECTOR_BYTES, SeekOrigin.Begin);

                int length = br.ReadInt32();
                byte compressionType = br.ReadByte();

                byte[] compressed = br.ReadBytes(length - 1);

                byte[] raw;
                using (var ms = new MemoryStream(compressed))
                using (var z = new System.IO.Compression.ZLibStream(ms, CompressionMode.Decompress))
                using (var outMs = new MemoryStream())
                {
                    z.CopyTo(outMs);
                    raw = outMs.ToArray();
                }

                return DeserializeChunk(raw, chunkX, chunkZ);
            }
            finally
            {
                regionLock.Release();
            }
        }

        private byte[] SerializeChunk(ServerChunk chunk)
        {
            var data = chunk.ChunkData;

            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);

            // Write dimensions
            bw.Write(data.SizeX);
            bw.Write(data.SizeY);
            bw.Write(data.SizeZ);

            var palette = new Dictionary<VoxelType, int>();
            var reversePalette = new List<VoxelType>();

            foreach (var voxel in data.Voxels)
            {
                if (!palette.ContainsKey((VoxelType)voxel))
                {
                    palette[(VoxelType)voxel] = reversePalette.Count;
                    reversePalette.Add((VoxelType)voxel);
                }
            }

            // Write palette
            bw.Write(reversePalette.Count);
            foreach (var type in reversePalette)
                bw.Write((int)type);

            int paletteSize = reversePalette.Count;
            int bitsPerBlock = Math.Max(1, (int)Math.Ceiling(Math.Log2(paletteSize)));

            bw.Write(bitsPerBlock);

            int totalBlocks = data.Voxels.Length;
            ulong buffer = 0;
            int bitsInBuffer = 0;

            foreach (var voxel in data.Voxels)
            {
                int id = palette[(VoxelType)voxel];

                buffer |= ((ulong)id << bitsInBuffer);
                bitsInBuffer += bitsPerBlock;

                while (bitsInBuffer >= 8)
                {
                    bw.Write((byte)(buffer & 0xFF));
                    buffer >>= 8;
                    bitsInBuffer -= 8;
                }
            }

            if (bitsInBuffer > 0)
                bw.Write((byte)(buffer & 0xFF));

            return ms.ToArray();
        }

        private ServerChunk DeserializeChunk(byte[] raw, int chunkX, int chunkZ)
        {
            using var ms = new MemoryStream(raw);
            using var br = new BinaryReader(ms);

            int sizeX = br.ReadInt32();
            int sizeY = br.ReadInt32();
            int sizeZ = br.ReadInt32();

            // Convert chunk index back to world origin
            Vector2 worldOrigin = new Vector2(
                chunkX * ChunkSize,
                chunkZ * ChunkSize
            );

            var chunkData = new ChunkData(
                sizeX,
                sizeY,
                sizeZ,
                worldOrigin
            );


            // Read palette
            int paletteCount = br.ReadInt32();
            var palette = new VoxelType[paletteCount];

            for (int i = 0; i < paletteCount; i++)
                palette[i] = (VoxelType)br.ReadInt32();

            int bitsPerBlock = br.ReadInt32();

            int totalBlocks = sizeX * sizeY * sizeZ;

            ulong buffer = 0;
            int bitsInBuffer = 0;

            for (int i = 0; i < totalBlocks; i++)
            {
                while (bitsInBuffer < bitsPerBlock)
                {
                    buffer |= ((ulong)br.ReadByte() << bitsInBuffer);
                    bitsInBuffer += 8;
                }

                int paletteIndex = (int)(buffer & ((1UL << bitsPerBlock) - 1));
                buffer >>= bitsPerBlock;
                bitsInBuffer -= bitsPerBlock;

                chunkData.Voxels[i] = (byte)palette[paletteIndex];
            }

            return new ServerChunk(chunkData);
        }

        #endregion

        #region Chunk Management

        private HashSet<Vector2> GetAllChunksRequiredByPlayers()
        {
            var required = new HashSet<Vector2>();
            int viewDistance = PlayerSettings.MaxChunkLoadDistance;

            foreach (var client in GameServer.Instance.ClientInstances.Values)
            {
                var pos = client.Position;

                int chunkX = (int)MathF.Floor(pos.X / ChunkSize);
                int chunkZ = (int)MathF.Floor(pos.Z / ChunkSize);

                for (int x = -viewDistance; x <= viewDistance; x++)
                {
                    for (int z = -viewDistance; z <= viewDistance; z++)
                    {
                        required.Add(new Vector2(chunkX + x, chunkZ + z));
                    }
                }
            }

            return required;
        }

        public void CleanupUnusedChunks()
        {
            var requiredChunks = GetAllChunksRequiredByPlayers();

            foreach (var kvp in CurrentChunks)
            {
                if (!requiredChunks.Contains(kvp.Key))
                {
                    if (CurrentChunks.TryRemove(kvp.Key, out _))
                    {
                        Console.WriteLine($"Unloaded chunk {kvp.Key}");
                    }
                }
            }
        }

        private double _lastCleanup;

        public void Update(double time)
        {
            if (time - _lastCleanup > 5.0)
            {
                CleanupUnusedChunks();
                _lastCleanup = time;
            }
        }

        #endregion
    }
}
