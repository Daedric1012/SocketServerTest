using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace ServerLibrary
{

    public class BuffStream
    {
        public NetworkStream stream { get; set; }
        public byte[] buffer { get; set; }
        public ClientUser ClientUser { get; set; }

        public BuffStream(NetworkStream s, byte[] b, ClientUser c)
        {
            stream = s;
            buffer = b;
            this.ClientUser = c;
        }
    }
}
