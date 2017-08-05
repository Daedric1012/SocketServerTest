namespace SocketClient
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Net.Sockets;
    using System.Threading;

    using ServerLibrary;

    internal class Client
    {
        private readonly TcpClient client = new TcpClient();

        private BackgroundWorker bw = new BackgroundWorker();

        public static void EndRead(IAsyncResult ar)
        {
            var bs = (BuffStream)ar.AsyncState;
            bs.stream.EndRead(ar);
            var buffer = bs.buffer;
            var msgLengthBytes = new byte[sizeof(int)];

            // grab the size of the payload.
            Buffer.BlockCopy(buffer, 0, msgLengthBytes, 0, msgLengthBytes.Length);
            var size = BitConverter.ToInt32(msgLengthBytes, 0);
            var payload = new byte[size];

            // grab the payload using the size
            Buffer.BlockCopy(buffer, sizeof(int) - 1, payload, 0, size);

            // convert it to an actual Words object
            var msg = (Message)MyByteConverter.ByteArrayToObject(payload);

            // OutBound.SendTextMessageOff(msg);
            // Console.WriteLine("test");
            if (msg != null)
            {
                Console.WriteLine("{0}:{1}", msg.PlayerId, msg.Words);
            }
        }
        
        [SuppressMessage("StyleCop.CSharp.LayoutRules", "SA1503:CurlyBracketsMustNotBeOmitted", Justification = "Reviewed. Suppression is OK here.")]
        private static void IncomingMessages(object obj)
        {
            // cast object to a stream
            var stream = (NetworkStream)obj;

            // buffer, change size later if needed.
            var buffer = new byte[1024];

            var errorCount = 0;

            while (true)
            { 
                try
                {
                    stream.BeginRead(buffer, 0, buffer.Length, EndRead, new BuffStream(stream, buffer, null));
                    buffer = new byte[1024];
                    errorCount = 0;
                }
                catch (Exception e)
                {
                    if (errorCount == 0)
                    {
                        Console.WriteLine("error: " + e);
                    }

                    errorCount++;
                    if (errorCount <= 10) continue;
                    Console.WriteLine("Disconnected from server incoming Words");
                    break;
                }
            }
        }

        private static void Main(string[] args)
        {
            Console.Title = "ClientUser";

            // Setup();
            var client = new Client();
            client.Setup();
            Console.ReadLine();
        }

        private static void SendMessage(NetworkStream stream, TcpClient client)
        {
            Console.Write("Enter Name:");
            var usrId = Console.ReadLine();
            while (client.Connected)

                try
                {
                    //Console.Write("{0}: ", usrId);
                    var input = Console.ReadLine();
                    var msg = new Message(input, MessageType.Async, usrId);
                    var buffer = new byte[1024];
                    var byteMessage = MyByteConverter.ObjectToByteArray(msg);

                    // Console.WriteLine("converted");
                    var size = byteMessage.Length;
                    var lengthBytes = BitConverter.GetBytes(size);
                    Buffer.BlockCopy(lengthBytes, 0, buffer, 0, lengthBytes.Length);
                    Buffer.BlockCopy(byteMessage, 0, buffer, lengthBytes.Length - 1, byteMessage.Length);
                    stream.Write(buffer, 0, buffer.Length);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Disconnected from server endread");
                }
        }

        private void Setup()
        {
            // connect to server
            Console.WriteLine("attempting to connect");
            while (!this.client.Connected)
            {
                // ClientUser.Connect("203.51.123.211", 3000);
                this.client.Connect("localhost", 3000);
            }

            // get the stream to write and read from.
            var stream = this.client.GetStream();

            Console.WriteLine("connected!");

            // have an open incoming stream to always accept messages from the server
            var t = new Thread(IncomingMessages);
            t.Start(stream);

            // start the send Words loop
            SendMessage(stream, this.client);
        }
    }
}