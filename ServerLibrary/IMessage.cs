

namespace ServerLibary
{
    public interface IMessage
    {
        int size { get; set; }
        string playerID { get; set; }
        string message { get; set; }
        MessageType messageType { get; set; }
    }
}
