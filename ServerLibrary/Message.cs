namespace ServerLibrary
{
    using System;

    [Serializable]
    public class Message : IMessage
    {
        public Message(string msg, MessageType mtype)
        {
            this.Words = msg;
            this.MessageType = mtype;
        }

        public string Words
        {
            get => this.Words;
            set => this.Words = value;
        }

        public MessageType MessageType { get; set; }

        public string PlayerId { get; set; }

        public int Size { get; set; }
    }
}