using System.Net.Sockets;
using System.Text;

namespace CubeEngine.Engine.Client
{
    public class GameClient
    {
        public static GameClient? Instance { get; private set; }

        public event Action<string>? OnServerMessage;

        private UdpClient _udpClient;
        private TcpClient? _tcpClient;
        private NetworkStream? _tcpStream;

        public GameClient(string address, int udpPort, int tcpPort)
        {
            // UDP setup
            _udpClient = new UdpClient();
            _udpClient.Connect(address, udpPort);

            // TCP setup
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
                string response = Encoding.UTF8.GetString(result.Buffer);
                OnServerMessage?.Invoke($"[UDP] {response}");
            }
        }

        public async Task SendUdpMessage(string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
            await _udpClient.SendAsync(data, data.Length);
        }
        #endregion

        #region TCP
        private async Task ReceiveTcpLoop()
        {
            byte[] buffer = new byte[1024];

            while (_tcpClient != null && _tcpClient.Connected)
            {
                int byteCount = await _tcpStream!.ReadAsync(buffer);
                if (byteCount == 0) break;

                string response = Encoding.UTF8.GetString(buffer, 0, byteCount);
                OnServerMessage?.Invoke($"[TCP] {response}");
            }
        }

        public async Task SendTcpMessage(string message)
        {
            if (_tcpStream != null && _tcpClient!.Connected)
            {
                byte[] data = Encoding.UTF8.GetBytes(message);
                await _tcpStream.WriteAsync(data);
            }
        }
        #endregion
    }
}