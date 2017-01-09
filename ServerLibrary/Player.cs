using System;

namespace ServerLibary
{
    [Serializable]
    public class Player : IMessage
    {
        public string _playerID { get; set; }
        public string _message { get; set; }
        public MessageType _messageType { get; set; }

        public Player(string msg, MessageType mtype, string playerID)
        {
            _playerID = playerID;
            _message = msg;
            _messageType = mtype;
        }
    }
}
