using System;

namespace ServerLibary
{
    [Serializable]
    public class Player : IMessage
    {
        public string playerID { get; set; }
        public string message { get; set; }
        public MessageType messageType { get; set; }

        public Player(string msg, MessageType mtype, string pID)
        {
            playerID = pID;
            message = msg;
            messageType = mtype;
        }
    }
}
