using ServerLibary;
using System;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;

namespace SocketServer
{
    class ServerStart
    {
        //list of our clients. singleton
        private static ClientList _clientList = ClientList.Instance;
        private static TcpListener _listener = null;
        BackgroundWorker bw = new BackgroundWorker();

        static void Main(string[] args)
        {
            Console.Title = "Server";
            //DataBase db = new DataBase();
            SetupServer();
            Console.ReadLine();
        }

        private async static void SetupServer()
        {
            Console.WriteLine("Setting up server...");
            _listener = new TcpListener(IPAddress.Any, 3000);
            _listener.Start();

            bool listening = true;
            while (listening)
            {
                //non blocking await, so can accept multiple clients
                TcpClient client = await _listener.AcceptTcpClientAsync().ConfigureAwait(false);
                Client c = new Client(client);
                _clientList.Add(c);
                Console.WriteLine("client connected");
                //handle client in new thread
                Thread t = new Thread(new ParameterizedThreadStart(HandleClient));
                t.Start(c);
            }
        }

        //new parameterizedthreadstart only allows obj
        public static void HandleClient(object obj)
        {
            //buffer, change size later if needed.
            byte[] buffer = new byte[1024];

            //cast obj to a tcp client
            Client client = (Client)obj;
            
            // Get a stream object for reading and writing
            NetworkStream stream = client.tcp.GetStream();
            while (client.tcp.Connected)
            {
                try {
                    IMessage msg;

                    IFormatter formatter = new BinaryFormatter();

                    msg = (IMessage)formatter.Deserialize(stream);
                    
                    //should adjust this to be switch with types.
                    if(msg is Player)
                    {
                        Player player = (Player)msg;
                    }else if(msg is Message)
                    {
                        Message message = (Message)msg;
                        Console.WriteLine("Received: {0}", message._message);
                    }
                    OutBound.SendTextMessage(msg);
                }
                catch(Exception e)
                {
                    //Console.WriteLine("error: " + e);
                }
            }
            //if client is disconnected remove it.
            client.tcp.Close();
            _clientList.Remove(client);
            Console.WriteLine("client disconnected");
        }
    }
}
