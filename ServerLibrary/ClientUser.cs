
namespace ServerLibrary
{
    using System.Net.Sockets;

    public class ClientUser
    {
        public ClientUser(TcpClient me)
        {
            this.Tcp = me;
        }

        public string ClientName { get; set; }

        public TcpClient Tcp { get; set; }
    }
}