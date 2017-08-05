
namespace SocketServer
{
    using System;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;

    using ServerLibrary;

    using ServerLibrary;

    // outbound messages will be sent from here!
    public class OutBound
    {
        private static readonly ClientList ClientList = ClientList.Instance;

        public static void SendSingleMessage(IMessage msg, Client client)
        {
            // Get a stream object
            var stream = client.Tcp.GetStream();

            IFormatter formatter = new BinaryFormatter();

            // write to out stream.
            formatter.Serialize(stream, msg);
        }

        public static void SendTextMessageOff(IMessage msg)
        {
            foreach (var client in ClientList)
            {
                // Get a stream object for reading and writing
                var stream = client.Tcp.GetStream();

                // IFormatter formatter = new BinaryFormatter();
                // write to out stream.
                // formatter.Serialize(stream, msg);
                var buffer = new byte[1024];
                var byteMessage = MyByteConverter.ObjectToByteArray(msg);

                // Console.WriteLine("converted");
                var size = byteMessage.Length;
                var lengthBytes = BitConverter.GetBytes(size);
                Buffer.BlockCopy(lengthBytes, 0, buffer, 0, lengthBytes.Length);
                Buffer.BlockCopy(byteMessage, 0, buffer, lengthBytes.Length - 1, byteMessage.Length);
                stream.BeginWrite(buffer, 0, buffer.Length, EndWrite, new BuffStream(stream, buffer, null));
            }
        }

        private static void EndWrite(IAsyncResult ar)
        {
            var bs = (BuffStream)ar.AsyncState;
            bs.stream.EndWrite(ar);
        }
    }
}