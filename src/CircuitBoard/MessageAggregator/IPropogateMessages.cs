using System;
using CircuitBoard.Messages;

namespace CircuitBoard.MessageAggregator
{
    /// <summary>
    ///     required because of reference by IMessageAggregator
    ///     We may want to return different version in different message aggregators, for example a No-Op implementation if
    ///     the library is used to gate functions in an application which is not circuitboard aware
    /// </summary>
    public interface IPropogateMessages<out TMessage>
        where TMessage : IMessage
    {
        void To(Action<TMessage> passTo);

        TOut To<TOut>(Func<TMessage, TOut> passTo);
    }
}