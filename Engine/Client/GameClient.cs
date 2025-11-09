using CubeEngine.Engine.Network;
using System.Net.Sockets;

namespace CubeEngine.Engine.Client
{
    public class GameClient
    {
        public static GameClient? Instance { get; private set; }

        public event Action<Packet>? OnServerMessage;

        private UdpClient _udpClient;
        private TcpClient? _tcpClient;
        private NetworkStream? _tcpStream;

        public GameClient(string address, int udpPort, int tcpPort)
        {
            _udpClient = new UdpClient();
            _udpClient.Connect(address, udpPort);

            _tcpClient = new TcpClient();
            _tcpClient.Connect(address, tcpPort);
            _tcpStream = _tcpClient.GetStream();

            Instance = this;
        }

        public void Start()
        {
            _ = ReceiveUdpLoop();
            _ = ReceiveTcpLoop();
        }

        #region UDP
        private async Task ReceiveUdpLoop()
        {
            while (true)
            {
                UdpReceiveResult result = await _udpClient.ReceiveAsync();
                byte[] buffer = result.Buffer;

                try
                {
                    Packet packet = Packet.Deserialize(buffer);
                    OnServerMessage?.Invoke(packet);
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine($"Failed to deserialize UDP packet: {e}");
                }
            }
        }

        public async Task SendUdpMessage(Packet packet)
        {
            if (_udpClient != null)
            {
                byte[] data = packet.Serialize();
                await _udpClient.SendAsync(data, data.Length);
            }
        }
        #endregion

        #region TCP
        private async Task ReceiveTcpLoop()
        {
            byte[] lengthBuffer = new byte[4];

            while (_tcpClient != null && _tcpClient.Connected)
            {
                int read = await _tcpStream.ReadAsync(lengthBuffer, 0, 4);
                if (read == 0) break;

                int packetLength = BitConverter.ToInt32(lengthBuffer, 0);

                byte[] packetBuffer = new byte[packetLength];
                int offset = 0;

                while (offset < packetLength)
                {
                    int bytesRead = await _tcpStream.ReadAsync(packetBuffer, offset, packetLength - offset);
                    if (bytesRead == 0) break; 
                    offset += bytesRead;
                }

                try
                {
                    Packet packet = Packet.Deserialize(packetBuffer);
                    OnServerMessage?.Invoke(packet);
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine($"Failed to deserialize TCP packet: {e}");
                }
            }
        }

        public async Task SendTcpMessage(Packet packet)
        {
            if (_tcpStream != null && _tcpClient!.Connected)
            {
                byte[] data = packet.Serialize();
                byte[] lengthPrefix = BitConverter.GetBytes(data.Length);

                await _tcpStream.WriteAsync(lengthPrefix, 0, lengthPrefix.Length);
                await _tcpStream.WriteAsync(data, 0, data.Length);
                await _tcpStream.FlushAsync();
            }
        }
        #endregion
    }
}