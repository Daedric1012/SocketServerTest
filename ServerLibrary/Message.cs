namespace ServerLibrary
{
    using System;

    [Serializable]
    public class Message : IMessage
    {
        public Message(string msg, MessageType mtype, string pId)
        {
            this.Words = msg;
            this.MessageType = mtype;
            this.PlayerId = pId;
        }

        public string Words { get; set; }

        public MessageType MessageType { get; set; }

        public string PlayerId { get; set; }

        public int Size { get; set; }
    }
}