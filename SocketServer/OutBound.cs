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

        public static void SendTextMessageOff(IMessage msg)
        {
            foreach (var client in _clientList)
            {
                // Get a stream object for reading and writing
                NetworkStream stream = client.tcp.GetStream();

                //IFormatter formatter = new BinaryFormatter();
                //write to out stream.
                //formatter.Serialize(stream, msg);

                byte[] buffer = new byte[1024];
                byte[] bMsg = MyByteConverter.ObjectToByteArray(msg);
                //Console.WriteLine("converted");
                int size = bMsg.Length;
                byte[] lengthBytes = BitConverter.GetBytes(size);
                Buffer.BlockCopy(lengthBytes, 0, buffer, 0, lengthBytes.Length);
                Buffer.BlockCopy(bMsg, 0, buffer, lengthBytes.Length - 1, bMsg.Length);
                stream.BeginWrite(buffer, 0, buffer.Length, EndWrite, new BuffStream(stream, buffer));
            }
        }

        static void EndWrite(IAsyncResult ar)
        {
            BuffStream bs = (BuffStream)ar.AsyncState;
            bs.stream.EndWrite(ar);
        }

        public static void SendSingleMessage(IMessage msg, Client client)
        {
            // Get a stream object
            NetworkStream stream = client.tcp.GetStream();

            IFormatter formatter = new BinaryFormatter();
            //write to out stream.
            formatter.Serialize(stream, msg);
        }
    }
}
