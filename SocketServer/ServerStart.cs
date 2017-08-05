namespace SocketServer
{
    using System;
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

        // Thread signal.
        private static readonly ManualResetEvent AllDone = new ManualResetEvent(false);

        // list of our clients. singleton
        private static readonly ClientList ClientList = ClientList.Instance;

        private static readonly ManualResetEvent MessageDone = new ManualResetEvent(false);

        private static TcpListener listener;

        private readonly IPAddress ipAddress = IPAddress.Any;

        public static void BeginRead(BuffStream c)
        {
            MessageDone.Reset();

            // buffer, change size later if needed.
            var buffer = new byte[1024];
            c.buffer = buffer;
            c.stream.BeginRead(buffer, 0, buffer.Length, EndRead, c);
            MessageDone.WaitOne();
        }

        // end the stream reading, handle the Words
        public static void EndRead(IAsyncResult ar)
        {
            // Console.WriteLine("EndRead Called");
            var bs = (BuffStream)ar.AsyncState;

            // grab a reference to the client to confirm username
            var obj = ClientList.FirstOrDefault(x => x == bs.ClientUser);
            try
            {
                var bytesRead = bs.stream.EndRead(ar);
                MessageDone.Set();
                var buffer = bs.buffer;
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
                    var msg = (Message)MyByteConverter.ByteArrayToObject(payload);

                    if (obj != null && obj.ClientName == null)
                    {
                        obj.ClientName = msg.PlayerId;
                    }

                    // broadcast
                    OutBound.SendTextMessageOff(msg);
                    Console.WriteLine("{0}: {1}", msg.PlayerId, msg.Words);
                }

                BeginRead(bs);
            }
            catch (IOException)
            {
                // will terminate the ClientUser connection when an error is thrown.
                // not sure on the best way to handle this
                Console.WriteLine("IOException: ClientUser being removed");
                bs.ClientUser.Tcp.Close();
                ClientList.Remove(bs.ClientUser);
            }
        }

        public static void HandleClient(IAsyncResult ar)
        {
            // Get the listener that handles the ClientUser request.
            var tcpL = (TcpListener)ar.AsyncState;
            var cli = tcpL.EndAcceptTcpClient(ar);
            var clientUser = new ClientUser(cli);
            ClientList.Add(clientUser);

            // Signal the main thread to continue.
            AllDone.Set();

            // Get a stream object for reading and writing
            var stream = clientUser.Tcp.GetStream();
            BeginRead(new BuffStream(stream, null, clientUser));
        }

        private static void Main(string[] args)
        {
            Console.Title = "Server";
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
                AllDone.Reset();

                // Start an asynchronous socket to listen for connections.
                // Console.WriteLine("Waiting for a connection...");
                listener.BeginAcceptTcpClient(HandleClient, listener);

                // Wait until a connection is made before continuing.
                AllDone.WaitOne();
            }
        }
    }
}