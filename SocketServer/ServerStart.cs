using ServerLibary;
using System;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;

//stuff

namespace SocketServer
{

    class ServerStart
    {
        // Thread signal.
        public static ManualResetEvent allDone = new ManualResetEvent(false);
        //list of our clients. singleton
        private static ClientList _clientList = ClientList.Instance;
        private static TcpListener _listener = null;
        BackgroundWorker bw = new BackgroundWorker();

        static void Main(string[] args)
        {
            Console.Title = "Server";
            //DataBase db = new DataBase();
            ServerStart server = new ServerStart();
            server.SetupServer();
            //SetupServer();
            Console.ReadLine();
        }

        private void SetupServer()
        {
            Console.WriteLine("Setting up server...");
            _listener = new TcpListener(IPAddress.Any, 3000);
            _listener.Start();

            bool listening = true;
            while (listening)
            {
                // Set the event to nonsignaled state.
                allDone.Reset();

                // Start an asynchronous socket to listen for connections.
                //Console.WriteLine("Waiting for a connection...");
                _listener.BeginAcceptTcpClient(HandleClient, _listener);

                // Wait until a connection is made before continuing.
                allDone.WaitOne();
            }
        }

        //new parameterizedthreadstart only allows obj
        public static void HandleClient(IAsyncResult ar)
        {
            // Get the listener that handles the client request.
            _listener = (TcpListener)ar.AsyncState;
            Client client = new Client(_listener.EndAcceptTcpClient(ar));
            _clientList.Add(client);

            // Signal the main thread to continue.
            allDone.Set();

            //buffer, change size later if needed.
            byte[] buffer = new byte[1024];

            // Get a stream object for reading and writing
            NetworkStream stream = client.tcp.GetStream();
            while (client.tcp.Connected)
            {
                try
                {
                    IMessage msg;

                    IFormatter formatter = new BinaryFormatter();

                    msg = (IMessage)formatter.Deserialize(stream);

                    //should adjust this to be switch with types.
                    if (msg is Player)
                    {
                        Player player = (Player)msg;
                    }
                    else if (msg is Message)
                    {
                        Message message = (Message)msg;
                        Console.WriteLine("Received: {0}", message.message);
                    }
                    OutBound.SendTextMessage(msg);
                }
                catch (Exception e)
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
