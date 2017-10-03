using CircuitBoard.Messages;

namespace CircuitBoard.MessageAggregator
{
    /// <summary>
    /// Gives us the ability to set "message mocks" so messages collect using
    /// CollectAndForward() are not actually processed during testing
    /// </summary>
    public interface IMockGatedFunctions
    {
        IValueReturner When<TEvent>() where TEvent : IMessage;
    }
}