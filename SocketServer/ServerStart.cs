using ServerLibary;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
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
        //BackgroundWorker bw = new BackgroundWorker();

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


        public static void HandleClient(IAsyncResult ar)
        {
            // Get the listener that handles the client request.
            TcpListener tcpL = (TcpListener)ar.AsyncState;
            TcpClient cli = tcpL.EndAcceptTcpClient(ar);
            Client client = new Client(cli);
            _clientList.Add(client);

            // Signal the main thread to continue.
            allDone.Set();

            //buffer, change size later if needed.
            byte[] buffer = new byte[1024];

            // Get a stream object for reading and writing
            NetworkStream stream = client.tcp.GetStream();

            int errorCount = 0;
            bool connected = true;
            while (connected)
            {
                try
                {
                    if (stream.CanRead)
                    {

                        stream.BeginRead(buffer, 0, buffer.Length, EndRead, new BuffStream(stream, buffer));
                        buffer = new byte[1024];
                        errorCount = 0;
                    }
                }
                catch (Exception e)
                {
                    if (errorCount == 0)
                        Console.WriteLine("error: " + e);

                    connected = false;
                    errorCount++;
                    if (errorCount > 10)
                    {
                        Console.WriteLine("Disconnected");
                        return;
                    }
                }

            }
            //if client is disconnected remove it.
            client.tcp.Close();
            _clientList.Remove(client);
            Console.WriteLine("client disconnected");
            connected = false;
            return;
        }

        //end the stream reading, handle the message
        public static void EndRead(IAsyncResult ar)
        {
            BuffStream bs = (BuffStream)ar.AsyncState;
            try
            {
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

                OutBound.SendTextMessageOff(msg);
                //Console.WriteLine("test");

                Console.WriteLine("Received: {0}", msg.message);
            }
            catch (IOException e)
            {
                bs.stream.Close();
                throw;
            }
        }


    }
}
