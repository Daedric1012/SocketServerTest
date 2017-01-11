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

        public BuffStream(NetworkStream s, byte[] b)
        {
            stream = s;
            buffer = b;
        }
    }
}
