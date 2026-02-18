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
            Instance = this;

            // TCP setup 
            _tcpListener = new TcpListener(IPAddress.Any, tcpPort);
            _tcpListener.Start();
            Console.WriteLine($"TCP Server started on port {tcpPort}");

            // UDP setup 
            _udpServer = new UdpClient(); // create unbound socket

            _udpServer.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            const int SIO_UDP_CONNRESET = -1744830452;
            _udpServer.Client.IOControl(SIO_UDP_CONNRESET, new byte[] { 0 }, null);

            // Bind to desired port
            _udpServer.Client.Bind(new IPEndPoint(IPAddress.Any, udpPort));
            Console.WriteLine($"UDP Server started on port {udpPort}");

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
                    Packet packet = Packet.Deserialize(result.Buffer);

                    // Register first packet from client
                    if (!ClientsByEndpoint.ContainsKey(result.RemoteEndPoint))
                    {
                        var client = ClientInstances.Values
                            .FirstOrDefault(c => c.TcpClient.Client.RemoteEndPoint is IPEndPoint ep &&
                                                 ep.Address.Equals(result.RemoteEndPoint.Address));
                        if (client != null)
                        {
                            client.UdpEndPoint = result.RemoteEndPoint;
                            ClientsByEndpoint[result.RemoteEndPoint] = client;
                            Console.WriteLine($"Registered UDP client: {client.Username} at {result.RemoteEndPoint}");
                        }
                    }

                    ClientMessage?.Invoke(result.RemoteEndPoint, packet);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"UDP Server error: {ex.Message}");
                }
            }
        }

        public void SendUdpMessage(ClientInstance client, Packet packet)
        {
            if (client.UdpEndPoint == null) return;

            byte[] data = packet.Serialize();
            _udpServer.Send(data, data.Length, client.UdpEndPoint);
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
            if (!client.Connected || !ClientInstances.TryGetValue(client, out var instance))
                return;

            await instance.TcpSendLock.WaitAsync();
            try
            {
                byte[] data = packet.Serialize();
                byte[] lengthPrefix = BitConverter.GetBytes(data.Length);

                byte[] fullPacket = new byte[lengthPrefix.Length + data.Length];
                Buffer.BlockCopy(lengthPrefix, 0, fullPacket, 0, lengthPrefix.Length);
                Buffer.BlockCopy(data, 0, fullPacket, lengthPrefix.Length, data.Length);

                NetworkStream stream = client.GetStream();
                await stream.WriteAsync(fullPacket, 0, fullPacket.Length);
                await stream.FlushAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending TCP to {instance.Username}: {ex.Message}");
            }
            finally
            {
                instance.TcpSendLock.Release();
            }
        }

        #endregion

        #region UDP Tick Sender

        private async Task TickLoop()
        {
            int delay = 1000 / _targetTick;

            const int tcpSendIntervalTicks = 60; 
            int tickCount = 0;

            while (_running)
            {
                try
                {
                    TickUpdate();
                    SendPlayerInfoTick(); 

                    if (tickCount % tcpSendIntervalTicks == 0)
                    {
                        SendPlayerInfoTickTCP();
                    }

                    tickCount++;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error during tick: {ex.Message}");
                }

                await Task.Delay(delay);
            }
        }

        private void SendPlayerInfoTick()
        {
            foreach (var client in ClientInstances.Values)
            {
                foreach (var info in ClientInstances.Values)
                {
                    PlayerStatePacket packet = new(info.Id, info.Position, info.Orientation, info.Head.Orientation);
                    SendUdpMessage(client, packet);
                }
            }
        }

        private void SendPlayerInfoTickTCP()
        {
            foreach (var client in ClientInstances.Values)
            {
                foreach (var info in ClientInstances.Values)
                {
                    PlayerStatePacket packet = new(info.Id, info.Position, info.Orientation, info.Head.Orientation);
                    try
                    {
                        var _ = SendTcpPacket(client.TcpClient, packet);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error sending TCP PlayerStatePacket to {client.Username}: {ex.Message}");
                    }
                }
            }
        }

        private void TickUpdate()
        {
            foreach (var client in ClientInstances.Values)
            {
                client.Update();
                ServerMap.Update(GameServer.ServerDeltaTime);
            }
        }

        #endregion

    }
}