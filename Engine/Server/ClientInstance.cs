using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace CubeEngine.Engine.Server
{
    public class ClientInstance
    {
        public Guid Id { get; } = Guid.NewGuid(); 
        public TcpClient TcpClient { get; }
        public IPEndPoint TcpEndPoint { get; }
        public DateTime ConnectedAt { get; } = DateTime.Now;

        public string Username { get; set; } = "Unknown"; 

        public ClientInstance(TcpClient client)
        {
            TcpClient = client;
            TcpEndPoint = (IPEndPoint)client.Client.RemoteEndPoint!;
        }

        public override string ToString()
        {
            return $"{Id} | {TcpEndPoint}";
        }
    }
}
