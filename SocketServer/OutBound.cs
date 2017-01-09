using ServerLibary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace SocketServer
{
    //outbound messages will be sent from here!
    public class OutBound
    {
        private static ClientList _clientList = ClientList.Instance;

        public static void SendTextMessage(IMessage msg)
        {
            //Console.WriteLine("sending: " + msg._message);
            foreach (var client in _clientList)
            {
                // Get a stream object for reading and writing
                NetworkStream stream = client.tcp.GetStream();

                IFormatter formatter = new BinaryFormatter();
                //write to out stream.
                formatter.Serialize(stream, msg);
            }
        }

        public static void SendSingleMessage(IMessage msg, Client client)
        {
            // Get a stream object
            NetworkStream stream = client.tcp.GetStream();

            IFormatter formatter = new BinaryFormatter();
            //write to out stream.
            formatter.Serialize(stream, msg);
        }

        //public OutBound() { }
    }
}
