using System;

namespace ServerLibary
{
    [Serializable]
    public class Message : IMessage
    {
        public int size { get; set; }
        public string playerID { get; set;}
        public string message { get; set; }
        public MessageType messageType { get; set; }

        public Message(string msg, MessageType mtype)
        {
            message = msg;
            messageType = mtype;
        }
    }
}
