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

namespace SocketServer
{
    class ServerStart
    {

        private readonly IPAddress _ipAddress = IPAddress.Any;
        private const int EndPort = 3000;
        // Thread signal.
        public static ManualResetEvent AllDone = new ManualResetEvent(false);
        public static ManualResetEvent MessageDone = new ManualResetEvent(false);
        //list of our clients. singleton
        private static readonly ClientList ClientList = ClientList.Instance;
        private static TcpListener _listener = null;
        //BackgroundWorker bw = new BackgroundWorker();

        private static void Main(string[] args)
        {
            Console.Title = "Server";
            //DataBase db = new DataBase();
            ServerStart server = new ServerStart();
            server.SetupServer();
            Console.ReadLine();
        }

        //not stactic because it only needs to be called once and from inside here
        private void SetupServer()
        {
            Console.WriteLine("Setting up server...");
            _listener = new TcpListener(_ipAddress, EndPort);
            _listener.Start();

            //bool listening = true;
            while (true)
            {
                // Set the event to nonsignaled state.
                AllDone.Reset();
                // Start an asynchronous socket to listen for connections.
                //Console.WriteLine("Waiting for a connection...");
                _listener.BeginAcceptTcpClient(HandleClient, _listener);
                // Wait until a connection is made before continuing.
                AllDone.WaitOne();
            }
        }

        public static void HandleClient(IAsyncResult ar)
        {
            // Get the listener that handles the client request.
            TcpListener tcpL = (TcpListener)ar.AsyncState;
            TcpClient cli = tcpL.EndAcceptTcpClient(ar);
            Client client = new Client(cli);
            ClientList.Add(client);

            // Signal the main thread to continue.
            AllDone.Set();

            // Get a stream object for reading and writing
            NetworkStream stream = client.tcp.GetStream();
            BeginRead(new BuffStream(stream, null, client));
        }

        public static void BeginRead(BuffStream c)
        {
            MessageDone.Reset();
            //buffer, change size later if needed.
            byte[] buffer = new byte[1024];
            c.buffer = buffer;
            c.stream.BeginRead(buffer, 0, buffer.Length, EndRead, c);
            MessageDone.WaitOne();
        }

        //end the stream reading, handle the message
        public static void EndRead(IAsyncResult ar)
        {
            //Console.WriteLine("EndRead Called");
            BuffStream bs = (BuffStream)ar.AsyncState;
            try
            {
                int bytesRead = bs.stream.EndRead(ar);
                MessageDone.Set();
                byte[] buffer = bs.buffer;
                byte[] msgLengthBytes = new byte[sizeof(int)];

                //grab the size of the payload.
                Buffer.BlockCopy(buffer, 0, msgLengthBytes, 0, msgLengthBytes.Length);
                int size = BitConverter.ToInt32(msgLengthBytes, 0);
                byte[] payload = new byte[size];

                //grab the payload using the size
                Buffer.BlockCopy(buffer, sizeof(int) - 1, payload, 0, size);
                //makes sure there is an actual payload. 
                if (!payload.SequenceEqual(new byte[size]))
                {
                    //convert it to an actual message object
                    Message msg = (Message)MyByteConverter.ByteArrayToObject(payload);
                    //broadcast
                    OutBound.SendTextMessageOff(msg);
                    Console.WriteLine("Received: {0}", msg.message);
                }
                BeginRead(bs);
            }
            //will terminate the client connection when an error is thrown.
            //not sure on the best way to handle this
            catch (IOException)
            {
                Console.WriteLine("IOException: client being removed");
                bs.client.tcp.Close();
                ClientList.Remove(bs.client);
            }
        }
    }
}
