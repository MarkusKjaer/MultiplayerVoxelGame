using System.Net;
using System.Net.Sockets;
using System.Text;

namespace CubeEngine.Engine.Server
{
    public class GameServer
    {
        public static GameServer? Instance { get; private set; }

        public event Action<IPEndPoint, string>? OnClientMessage;

        private readonly UdpClient _udpServer;
        private TcpListener? _tcpListener;
        private readonly List<TcpClient> _tcpClients = [];
        private bool _running;

        public GameServer(int udpPort, int tcpPort)
        {
            _udpServer = new UdpClient(udpPort);
            _tcpListener = new TcpListener(IPAddress.Any, tcpPort);

            Console.WriteLine($"UDP Server started on port {udpPort}");
            Console.WriteLine($"TCP Server started on port {tcpPort}");

            Instance = this;
        }

        public void Start()
        {
            _running = true;
            _ = ReceiveUdpLoop();
            _ = AcceptTcpClientsLoop();
        }

        public void Stop()
        {
            _running = false;
            _udpServer.Close();
            _tcpListener?.Stop();
            foreach (var client in _tcpClients)
                client.Close();
        }

        #region UDP

        private async Task ReceiveUdpLoop()
        {
            while (_running)
            {
                try
                {
                    UdpReceiveResult result = await _udpServer.ReceiveAsync();
                    string message = Encoding.UTF8.GetString(result.Buffer);

                    // Notify subscribers
                    OnClientMessage?.Invoke(result.RemoteEndPoint, message);
                }
                catch (ObjectDisposedException) { break; }
                catch (Exception ex) { Console.WriteLine($"UDP Server error: {ex.Message}"); }
            }
        }

        public void SendUdpMessage(IPEndPoint client, string message)
        {
            byte[] data = Encoding.UTF8.GetBytes(message);
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
                    _tcpClients.Add(tcpClient);

                    _ = HandleTcpClient(tcpClient);
                }
                catch (ObjectDisposedException) { break; }
                catch (Exception ex) { Console.WriteLine($"TCP Server error: {ex.Message}"); }
            }
        }

        private async Task HandleTcpClient(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];

            try
            {
                while (_running && client.Connected)
                {
                    int byteCount = await stream.ReadAsync(buffer);
                    if (byteCount == 0) break; // client disconnected

                    string message = Encoding.UTF8.GetString(buffer, 0, byteCount);
                    OnClientMessage?.Invoke((IPEndPoint)client.Client.RemoteEndPoint!, message);

                    Console.WriteLine(message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"TCP client error: {ex.Message}");
            }
            finally
            {
                client.Close();
                _tcpClients.Remove(client);
            }
        }

        public async Task SendTcpMessage(TcpClient client, string message)
        {
            if (client.Connected)
            {
                byte[] data = Encoding.UTF8.GetBytes(message);
                await client.GetStream().WriteAsync(data);
            }
        }

        #endregion
    }
}