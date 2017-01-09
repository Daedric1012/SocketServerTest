using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SocketServer
{
    public class Client
    {
        public TcpClient tcp { get; set; }

        public Client(TcpClient me)
        {
            tcp = me;
        }
    }
}
