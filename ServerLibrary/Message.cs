using System;

namespace ServerLibary
{
    [Serializable]
    public class Message : IMessage
    {
        public string _playerID { get; set;}
        public string _message { get; set; }
        public MessageType _messageType { get; set; }

        public Message(string msg, MessageType mtype)
        {
            _message = msg;
            _messageType = mtype;
        }
    }
}
