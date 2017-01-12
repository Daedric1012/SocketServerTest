using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace ServerLibary
{
    public class BuffStream
    {
        public NetworkStream stream { get; set; }
        public byte[] buffer { get; set; }
        public Client client { get; set; }

        public BuffStream(NetworkStream s, byte[] b, Client c)
        {
            stream = s;
            buffer = b;
            client = c;
        }
    }
}
