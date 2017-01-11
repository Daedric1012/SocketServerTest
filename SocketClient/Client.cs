using ServerLibary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SocketClient
{
    class Client
    {
        BackgroundWorker bw = new BackgroundWorker();

        static void Main(string[] args)
        {
            Console.Title = "Client";
            //Setup();
            Client client = new Client();
            client.Setup();
            Console.ReadLine();
        }

        private void Setup()
        {
            //connect to server
            TcpClient client = new TcpClient();
            Console.WriteLine("attempting to connect");
            while (!client.Connected)
            {
                client.Connect("203.51.123.211", 3000);
            }

            //get the stream to write and read from.
            NetworkStream stream = client.GetStream();

            Console.WriteLine("connected!");

            //have an open incoming stream to always accept messages from the server
            Thread t = new Thread(new ParameterizedThreadStart(IncomingMessages));
            t.Start(stream);

            //start the send message loop
            SendMessage(stream, client);
        }

        private static void SendMessage(NetworkStream stream, TcpClient client)
        {
            while (client.Connected)
            {
                string input = Console.ReadLine();
                Message msg = new Message(input, MessageType.Async);
                byte[] buffer = new byte[1024];
                byte[] bMsg = MyByteConverter.ObjectToByteArray(msg);
                //Console.WriteLine("converted");
                int size = bMsg.Length;
                byte[] lengthBytes = BitConverter.GetBytes(size);
                Buffer.BlockCopy(lengthBytes, 0, buffer, 0, lengthBytes.Length);
                Buffer.BlockCopy(bMsg, 0, buffer, lengthBytes.Length - 1, bMsg.Length);
                stream.Write(buffer, 0, buffer.Length);
            }
        }

        public static void EndRead(IAsyncResult ar)
        {
            BuffStream bs = (BuffStream)ar.AsyncState;
            bs.stream.EndRead(ar);
            byte[] buffer = bs.buffer;
            byte[] msgLengthBytes = new byte[sizeof(int)];

            //grab the size of the payload.
            Buffer.BlockCopy(buffer, 0, msgLengthBytes, 0, msgLengthBytes.Length);
            int size = BitConverter.ToInt32(msgLengthBytes, 0);
            byte[] payload = new byte[size];

            //grab the payload using the size
            Buffer.BlockCopy(buffer, sizeof(int) - 1, payload, 0, size);
            //convert it to an actual message object
            Message msg = (Message)MyByteConverter.ByteArrayToObject(payload);

            //OutBound.SendTextMessageOff(msg);
            //Console.WriteLine("test");

            Console.WriteLine("Received: {0}", msg.message);
        }

        private static void IncomingMessages(object obj)
        {
            //cast object to a stream
            NetworkStream stream = (NetworkStream)obj;

            //buffer, change size later if needed.
            byte[] buffer = new byte[1024];

            int errorCount = 0;

            while (true)
            {
                try
                {
                    stream.BeginRead(buffer, 0, buffer.Length, EndRead, new BuffStream(stream, buffer));
                    buffer = new byte[1024];
                    errorCount = 0;
                }
                catch (Exception e)
                {
                    if (errorCount == 0)
                        Console.WriteLine("error: " + e);

                    errorCount++;
                    if (errorCount > 10)
                    {
                        Console.WriteLine("Disconnected from server");
                        break;
                    }
                }
            }
        }
    }
}
