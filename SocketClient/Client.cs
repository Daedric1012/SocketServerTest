using ServerLibary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
            IFormatter formatter = new BinaryFormatter();
            while (client.Connected)
            {
                string input = Console.ReadLine();
                var msg = new Message(input, MessageType.Async);
                //write to out stream.
                var msg2 = (IMessage)msg;
                formatter.Serialize(stream, msg2);
            }
        }

        private static void IncomingMessages(object obj)
        {
            //cast object to a stream
            NetworkStream stream = (NetworkStream)obj;

            //buffer, change size later if needed.
            byte[] buffer = new byte[1024];

            while (true)
            {
                try
                {
                    //new message object
                    Message msg = new Message(null, MessageType.Async);
                    IFormatter formatter = new BinaryFormatter();

                    //recive message
                    msg = (Message)formatter.Deserialize(stream);

                    Console.WriteLine("Received: {0}", msg.message);
                    //BroadcastMessage(clientMessage);
                }
                catch (Exception e)
                {
                    Console.WriteLine("error: " + e);
                }
            }
        }
    }
}
