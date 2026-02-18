using CubeEngine.Engine.Network;
using System.Net;
using System.Net.Sockets;

namespace CubeEngine.Engine.Client
{
    public class GameClient
    {
        public static GameClient? Instance { get; private set; }

        public event Action<Packet>? ServerMessage;

        private UdpClient _udpClient;
        private TcpClient? _tcpClient;
        private NetworkStream? _tcpStream;

        private bool _running;

        public TaskCompletionSource<bool> ReadyTcs { get; } = new();

        public int ClientId { get; private set; } = -1;
        public string Name { get; private set; }

        private readonly string _serverAddress;
        private readonly int _udpPort;

        public GameClient(string address, int tcpPort, int udpPort)
        {
            _serverAddress = address;
            _udpPort = udpPort;

            // TCP setup
            _tcpClient = new TcpClient();
            _tcpClient.Connect(address, tcpPort);
            _tcpStream = _tcpClient.GetStream();

            // UDP setup
            _udpClient = new UdpClient();
            _udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, 0)); // OS picks free local port
            _udpClient.Connect(address, udpPort); // set default remote for SendAsync

            Console.WriteLine($"UDP client bound to {_udpClient.Client.LocalEndPoint} and connected to {address}:{udpPort}");

            Instance = this;

            ServerMessage += OnServerMessage;
        }

        public async Task StartAsync()
        {
            _running = true;
            Console.WriteLine("Client is starting...");

            var connectPacket = new ConnectPacket("TestName");
            await SendTcpMessage(connectPacket);

            ReadyTcs.SetResult(true);

            // Start UDP receive loop first
            _ = ReceiveUdpLoop();

            var hello = new PingPacket();
            await SendUdpMessage(hello);

            _ = ReceiveTcpLoop();
        }

        public void Stop()
        {
            _running = false;
            _tcpClient?.Close();
            _udpClient?.Close();
        }

        #region UDP
        private async Task ReceiveUdpLoop()
        {
            while (_running)
            {
                try
                {
                    UdpReceiveResult result = await _udpClient.ReceiveAsync();
                    Packet packet = Packet.Deserialize(result.Buffer);
                    ServerMessage?.Invoke(packet);
                }
                catch (Exception e)
                {
                    if (_running)
                        Console.Error.WriteLine($"Failed to deserialize UDP packet: {e}");
                }
            }
        }

        public async Task SendUdpMessage(Packet packet)
        {
            if (_running && _udpClient != null)
            {
                byte[] data = packet.Serialize();
                await _udpClient.SendAsync(data, data.Length); // endpoint already set
            }
        }
        #endregion

        #region TCP
        private async Task ReceiveTcpLoop()
        {
            byte[] lengthBuffer = new byte[4];

            while (_running && _tcpClient != null && _tcpClient.Connected)
            {
                int read;
                try
                {
                    read = await _tcpStream.ReadAsync(lengthBuffer, 0, 4);
                }
                catch
                {
                    break;
                }

                if (read == 0) break;

                int packetLength = BitConverter.ToInt32(lengthBuffer, 0);
                byte[] packetBuffer = new byte[packetLength];
                int offset = 0;

                while (_running && offset < packetLength)
                {
                    int bytesRead = await _tcpStream.ReadAsync(packetBuffer, offset, packetLength - offset);
                    if (bytesRead == 0) break;
                    offset += bytesRead;
                }

                try
                {
                    Packet packet = Packet.Deserialize(packetBuffer);
                    ServerMessage?.Invoke(packet);
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine($"TCP Stream desynced. Closing connection: {e}");
                    Stop();
                    break;
                }
            }
            Console.WriteLine("Stopped client");
        }

        public async Task SendTcpMessage(Packet packet)
        {
            if (_running && _tcpStream != null && _tcpClient!.Connected)
            {
                byte[] data = packet.Serialize();
                byte[] lengthPrefix = BitConverter.GetBytes(data.Length);

                byte[] fullPacket = new byte[lengthPrefix.Length + data.Length];
                Buffer.BlockCopy(lengthPrefix, 0, fullPacket, 0, lengthPrefix.Length);
                Buffer.BlockCopy(data, 0, fullPacket, lengthPrefix.Length, data.Length);

                await _tcpStream.WriteAsync(fullPacket, 0, fullPacket.Length);
                await _tcpStream.FlushAsync();
            }
        }
        #endregion

        private void OnServerMessage(Packet packet)
        {
            switch (packet)
            {
                case PlayerJoinConfirmPacket playerJoinConfirmPacket:
                    HandlePacket(playerJoinConfirmPacket);
                    break;
                default:
                    break;
            }
        }

        private void HandlePacket(PlayerJoinConfirmPacket playerJoinConfirmPacket)
        {
            if (ClientId == -1)
            {
                ClientId = playerJoinConfirmPacket.PlayerId;
                Name = playerJoinConfirmPacket.PlayerName;
            }
        }
    }
}