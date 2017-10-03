namespace DataStore.MessageAggregator
{
    using System.Collections.Generic;
    using CircuitBoard.MessageAggregator;
    using CircuitBoard.Messages;

    public class DataStoreMessageAggregator : IMessageAggregator
    {
        public Dictionary<string, object> ReturnValues = new Dictionary<string, object>();

        private readonly ReadOnlyCapableList<IMessage> allMessages = new ReadOnlyCapableList<IMessage>();

        public CircuitBoard.IReadOnlyList<IMessage> AllMessages => this.allMessages;

        public static IMessageAggregator Create()
        {
            return new DataStoreMessageAggregator();
        }

        public void Collect(IMessage message)
        {
            this.allMessages.Add(message);
        }

        public IPropogateMessages<TMessage> CollectAndForward<TMessage>(TMessage message) where TMessage : IMessage
        {
            this.allMessages.Add(message);
            return new DataStoreMessagePropogator<TMessage>(message);
        }

        public IValueReturner When<TMessage>() where TMessage : IMessage
        {
            return new DataStoreValueReturner(this.ReturnValues, typeof(TMessage).FullName);
        }
    }
}