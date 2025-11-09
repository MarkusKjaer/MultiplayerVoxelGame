using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CubeEngine.Engine.Network
{
    public class ConnectPacket : Packet
    {
        public ConnectPacket(byte[] buffer) : base(PacketType.Connect)
        {
        }
    }
}
