using CircuitBoard.Messages;

namespace CircuitBoard.MessageAggregator
{
    /// <summary>
    ///     We will want to expose this interface to library creators to register messages
    ///     and to add specialised collections for testing to different implementations
    /// </summary>
    public interface IMessageAggregator
    {
        IReadOnlyList<IMessage> AllMessages { get; }

        void Collect(IMessage message);

        IPropogateMessages<TEvent> CollectAndForward<TEvent>(TEvent message) where TEvent : IMessage;
    }
}