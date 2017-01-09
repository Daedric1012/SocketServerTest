

namespace ServerLibary
{
    public interface IMessage
    {
        string _playerID { get; set; }
        string _message { get; set; }
        MessageType _messageType { get; set; }
    }
}
