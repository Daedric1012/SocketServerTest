

namespace ServerLibary
{
    public interface IMessage
    {
        string playerID { get; set; }
        string message { get; set; }
        MessageType messageType { get; set; }
    }
}
