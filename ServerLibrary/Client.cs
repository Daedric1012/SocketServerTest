
namespace ServerLibrary
{
    using System.Net.Sockets;

    public class Client
    {
        public Client(TcpClient me)
        {
            this.Tcp = me;
        }

        public TcpClient Tcp { get; set; }
    }
}