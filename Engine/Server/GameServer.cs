using CubeEngine.Engine.Network;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace CubeEngine.Engine.Server
{
    public class GameServer
    {
        public static GameServer? Instance { get; private set; }

        public event Action<IPEndPoint, Packet>? OnClientMessage;

        private readonly UdpClient _udpServer;
        private TcpListener? _tcpListener;
        private readonly Dictionary<TcpClient, ClientInstance> _clientInstances = new();
        private bool _running;

        public ServerMap ServerMap { get; }

        public GameServer(int udpPort, int tcpPort)
        {
            _udpServer = new UdpClient(udpPort);
            _tcpListener = new TcpListener(IPAddress.Any, tcpPort);

            Console.WriteLine($"UDP Server started on port {udpPort}");
            Console.WriteLine($"TCP Server started on port {tcpPort}");

            Instance = this;

            ServerMap = new(32, 64, 1);
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
            foreach (var client in _clientInstances)
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
                    OnClientMessage?.Invoke(result.RemoteEndPoint, packet);
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
                    var clientInstance = new ClientInstance(tcpClient);
                    _clientInstances[tcpClient] = clientInstance;

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
                    OnClientMessage?.Invoke((IPEndPoint)client.Client.RemoteEndPoint!, packet);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"TCP client error: {ex.Message}");
            }
            finally
            {
                if (_clientInstances.ContainsKey(client))
                {
                    Console.WriteLine($"Client disconnected: {_clientInstances[client]}");
                    _clientInstances.Remove(client);
                }

                client.Close();
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
    }
}