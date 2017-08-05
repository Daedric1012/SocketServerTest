
namespace SocketServer
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using ServerLibrary;

    /// <summary>
    ///     The server start.
    /// </summary>
    public class ServerStart
    {
        private const int EndPort = 3000;

        // list of our clients. singleton
        private static readonly ClientList ClientList = ClientList.Instance;

        // Thread signal.
        private static ManualResetEvent allDone = new ManualResetEvent(false);

        private static ManualResetEvent messageDone = new ManualResetEvent(false);
        
        private static TcpListener listener;

        private readonly IPAddress ipAddress = IPAddress.Any;
        
        public static void BeginRead(BuffStream c)
        {
            messageDone.Reset();

            // buffer, change size later if needed.
            var buffer = new byte[1024];
            c.buffer = buffer;
            c.stream.BeginRead(buffer, 0, buffer.Length, EndRead, c);
            messageDone.WaitOne();
        }

        // end the stream reading, handle the Words
        public static void EndRead(IAsyncResult ar)
        {
            // Console.WriteLine("EndRead Called");
            BuffStream bs = (BuffStream)ar.AsyncState;
            try
            {
                int bytesRead = bs.stream.EndRead(ar);
                messageDone.Set();
                byte[] buffer = bs.buffer;
                var msgLengthBytes = new byte[sizeof(int)];

                // grab the size of the payload.
                Buffer.BlockCopy(buffer, 0, msgLengthBytes, 0, msgLengthBytes.Length);
                var size = BitConverter.ToInt32(msgLengthBytes, 0);
                var payload = new byte[size];

                // grab the payload using the size
                Buffer.BlockCopy(buffer, sizeof(int) - 1, payload, 0, size);

                // makes sure there is an actual payload. 
                if (!payload.SequenceEqual(new byte[size]))
                {
                    // convert it to an actual Words object
                    Message msg = (Message)MyByteConverter.ByteArrayToObject(payload);

                    // broadcast
                    OutBound.SendTextMessageOff(msg);
                    Console.WriteLine("Received: {0}", msg.Words);
                }
                BeginRead(bs);
            }
            catch (IOException)
            {
                // will terminate the client connection when an error is thrown.
                // not sure on the best way to handle this
                Console.WriteLine("IOException: client being removed");
                bs.client.Tcp.Close();
                ClientList.Remove(bs.client);
            }
        }

        public static void HandleClient(IAsyncResult ar)
        {
            // Get the listener that handles the client request.
            var tcpL = (TcpListener)ar.AsyncState;
            var cli = tcpL.EndAcceptTcpClient(ar);
            Client client = new Client(cli);
            ClientList.Add(client);

            // Signal the main thread to continue.
            allDone.Set();

            // Get a stream object for reading and writing
            NetworkStream stream = client.Tcp.GetStream();
            BeginRead(new BuffStream(stream, null, client));
        }

        private static void Main(string[] args)
        {
            Console.Title = "Server";

            // DataBase db = new DataBase();
            var server = new ServerStart();
            server.SetupServer();
            Console.ReadLine();
        }

        // not static because it only needs to be called once and from inside here
        private void SetupServer()
        {
            Console.WriteLine("Setting up server...");
            listener = new TcpListener(this.ipAddress, EndPort);
            listener.Start();

            while (true)
            {
                // Set the event to nonsignaled state.
                allDone.Reset();

                // Start an asynchronous socket to listen for connections.
                // Console.WriteLine("Waiting for a connection...");
                listener.BeginAcceptTcpClient(HandleClient, listener);

                // Wait until a connection is made before continuing.
                allDone.WaitOne();
            }
        }
    }
}