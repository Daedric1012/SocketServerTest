using ServerLibary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SocketServer
{
    //singleton on activly connected clients
    public class ClientList : List<Client>
    {
        private readonly static ClientList instance = new ClientList();
        
        private ClientList() { }

        public static ClientList Instance { get { return instance; } }
    }
}
