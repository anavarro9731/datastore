namespace DataStore.MessageAggregator
{
    using System.Collections.Generic;
    using CircuitBoard.MessageAggregator;
    using CircuitBoard.Messages;

    public class DataStoreMessageAggregator : IMessageAggregator
    {
        public Dictionary<string, object> ReturnValues = new Dictionary<string, object>();

        private readonly List<IMessage> allMessages = new List<IMessage>();

        public static IMessageAggregator Create() => new DataStoreMessageAggregator();

        public IReadOnlyList<IMessage> AllMessages => this.allMessages.AsReadOnly();

        public void Clear()
        {
            this.allMessages.Clear();
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

        public IValueReturner When<TMessage>() where TMessage : IMessage =>
            new DataStoreValueReturner(this.ReturnValues, typeof(TMessage).FullName);
    }
}