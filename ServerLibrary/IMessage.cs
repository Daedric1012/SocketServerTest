namespace ServerLibrary
{
    public interface IMessage
    {
        string Words { get; set; }

        MessageType MessageType { get; set; }

        string PlayerId { get; set; }

        int Size { get; set; }
    }
}