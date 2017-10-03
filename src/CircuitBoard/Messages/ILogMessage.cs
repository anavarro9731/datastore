namespace CircuitBoard.Messages
{
    public interface ILogMessage : IMessage
    {
        string Text { get; set; }
    }
}