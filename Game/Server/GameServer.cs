using CubeEngine.Engine.Network;
using MultiplayerVoxelGame.Util.Settings;
using OpenTK.Mathematics;
using System.Net;
using System.Net.Sockets;

namespace CubeEngine.Engine.Server
{
    public class GameServer
    {
        public static GameServer? Instance { get; private set; }

        private const int CHUNK_SIZE = ChunkSettings.Width;
        private const int CHUNK_HEIGHT = ChunkSettings.Height;
        private const int MAP_SEED = 1;

        public event Action<IPEndPoint, Packet>? ClientMessage;

        private readonly UdpClient _udpServer;
        private TcpListener? _tcpListener;
        public readonly Dictionary<TcpClient, ClientInstance> ClientInstances = new();
        public readonly Dictionary<IPEndPoint, ClientInstance> ClientsByEndpoint = new();
        private bool _running;

        public const int _targetTick = 60;
        public const float ServerDeltaTime = 1f / _targetTick;

        public TaskCompletionSource<bool> ReadyTcs { get; } = new();

        public ServerMap ServerMap { get; }

        public GameServer(int tcpPort, int udpPort)
        {
            _tcpListener = new TcpListener(IPAddress.Any, tcpPort);
            _udpServer = new UdpClient(udpPort);

            Console.WriteLine($"UDP Server started on port {udpPort}");
            Console.WriteLine($"TCP Server started on port {tcpPort}");

            Instance = this;

            string savePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Saves");
            ServerMap = new ServerMap(CHUNK_SIZE, CHUNK_HEIGHT, MAP_SEED);

            ClientMessage += OnClientMessage;
        }

        private void OnClientMessage(IPEndPoint client, Packet packet)
        {
            switch (packet)
            {
                case ConnectPacket connect:
                    ClientsByEndpoint[client].Username = connect.PlayerName;
                    PlayerJoinConfirmPacket playerJoinConfirmPacket = new(ClientsByEndpoint[client].Id, connect.PlayerName);
                    _ = SendTcpPacket(ClientsByEndpoint[client].TcpClient, playerJoinConfirmPacket);
                    break;
            }
        }

        public async Task StartAsync()
        {
            _running = true;

            _tcpListener?.Start();

            Console.WriteLine("Server is starting...");

            // Mark that server sockets are initialized
            ReadyTcs.TrySetResult(true);

            // Start background loops
            _ = TickLoop();
            _ = ReceiveUdpLoop();
            _ = AcceptTcpClientsLoop();
        }

        public void Stop()
        {
            _running = false;
            _udpServer.Close();
            _tcpListener?.Stop();
            foreach (var client in ClientInstances)
                client.Value.TcpClient.Close();
        }

        #region UDP

        private async Task ReceiveUdpLoop()
        {
            while (_running)
            {
                try
                {
                    UdpReceiveResult result = await _udpServer.ReceiveAsync();
                    byte[] buffer = result.Buffer;

                    Packet packet = Packet.Deserialize(buffer);
                    ClientMessage?.Invoke(result.RemoteEndPoint, packet);
                }
                catch (Exception ex) { Console.WriteLine($"UDP Server error: {ex.Message}"); }
            }
        }

        public void SendUdpMessage(IPEndPoint client, Packet packet)
        {
            byte[] data = packet.Serialize();
            _udpServer.Send(data, data.Length, client);
        }

        #endregion

        #region TCP

        private async Task AcceptTcpClientsLoop()
        {
            _tcpListener?.Start();

            while (_running)
            {
                try
                {
                    TcpClient tcpClient = await _tcpListener!.AcceptTcpClientAsync();
                    var clientInstance = new ClientInstance(tcpClient);

                    clientInstance.Setup(new(0, 60, 0));

                    ClientInstances[tcpClient] = clientInstance;

                    ClientsByEndpoint[clientInstance.EndPoint] = clientInstance;

                    Console.WriteLine($"New client connected: {clientInstance}");

                    _ = HandleTcpClient(tcpClient);
                }
                catch (ObjectDisposedException) { break; }
                catch (Exception ex) { Console.WriteLine($"TCP Server error: {ex.Message}"); }
            }
        }


        private async Task HandleTcpClient(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            byte[] lengthBuffer = new byte[4];

            try
            {
                while (_running && client.Connected)
                {
                    int read = await stream.ReadAsync(lengthBuffer, 0, 4);
                    if (read == 0) break;

                    int packetLength = BitConverter.ToInt32(lengthBuffer, 0);
                    byte[] packetBuffer = new byte[packetLength];
                    int offset = 0;

                    while (offset < packetLength)
                    {
                        int bytesRead = await stream.ReadAsync(packetBuffer, offset, packetLength - offset);
                        if (bytesRead == 0) break;
                        offset += bytesRead;
                    }

                    Packet packet = Packet.Deserialize(packetBuffer);
                    ClientMessage?.Invoke((IPEndPoint)client.Client.RemoteEndPoint!, packet);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"TCP client error: {ex.Message}");
            }
            finally
            {
                if (ClientInstances.ContainsKey(client))
                {
                    var ci = ClientInstances[client];
                    ClientsByEndpoint.Remove(ci.EndPoint);
                    ClientInstances.Remove(client);

                    Console.WriteLine($"Client disconnected: {ci}");
                }
            }
        }

        public async Task SendTcpPacket(TcpClient client, Packet packet)
        {
            if (client.Connected)
            {
                byte[] data = packet.Serialize();
                byte[] lengthPrefix = BitConverter.GetBytes(data.Length);

                await client.GetStream().WriteAsync(lengthPrefix, 0, lengthPrefix.Length);
                await client.GetStream().WriteAsync(data, 0, data.Length);
                await client.GetStream().FlushAsync();
            }
        }

        #endregion

        #region UDP Tick Sender

        private async Task TickLoop()
        {
            int delay = 1000 / _targetTick;

            while (_running)
            {
                try
                {
                    TickUpdate();
                    SendPlayerInfoTick();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending UDP tick: {ex.Message}");
                }

                await Task.Delay(delay);
            }
        }

        private void SendPlayerInfoTick()
        {
            foreach (var client in ClientInstances)
            {
                foreach (var clientInfoToSend in ClientInstances)
                {
                    PlayerStatePacket playerStatePacket = new(clientInfoToSend.Value.Id, clientInfoToSend.Value.Position, clientInfoToSend.Value.Orientation, clientInfoToSend.Value.Head.Orientation);
                    _ = SendTcpPacket(client.Value.TcpClient, playerStatePacket);
                }
            }
        }

        private void TickUpdate()
        {
            foreach (var client in ClientInstances.Values)
            {
                client.Update();
            }
        }

        #endregion

        #region Map

        private void DeloadChunks(List<ClientInstance> clientInstances, ServerMap serverMap)
        {
            List<Vector2> chunksToDeload = new();
            List<Vector2> playerChunkPositions = clientInstances.Select(ci =>
            {
                int chunkX = (int)MathF.Floor(ci.Position.X / CHUNK_SIZE);
                int chunkZ = (int)MathF.Floor(ci.Position.Z / CHUNK_SIZE);
                return new Vector2(chunkX, chunkZ);
            }).ToList();

            foreach (var item in playerChunkPositions)
            {
                
            }

        }

        #endregion
    }
}