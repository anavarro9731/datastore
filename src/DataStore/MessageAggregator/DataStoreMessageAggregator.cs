namespace DataStore.MessageAggregator
{
    using System.Collections.Generic;
    using ServiceApi.Interfaces.LowLevel.MessageAggregator;
    using ServiceApi.Interfaces.LowLevel.Messages;

    public class DataStoreMessageAggregator : IMessageAggregator
    {
        private readonly ReadOnlyCapableList<IMessage> _allMessages = new ReadOnlyCapableList<IMessage>();
        public Dictionary<string, object> ReturnValues = new Dictionary<string, object>();

        public ServiceApi.Interfaces.LowLevel.MessageAggregator.IReadOnlyList<IMessage> AllMessages => _allMessages;

        public void Collect(IMessage message)
        {
            _allMessages.Add(message);
        }

        public IPropogateMessages<TMessage> CollectAndForward<TMessage>(TMessage message) where TMessage : IMessage
        {
            _allMessages.Add(message);
            return new DataStoreMessagePropogator<TMessage>(
                message);
        }

        public IValueReturner When<TMessage>() where TMessage : IMessage
        {
            return new DataStoreValueReturner(ReturnValues, typeof(TMessage).FullName);
        }

        public static IMessageAggregator Create()
        {
            return new DataStoreMessageAggregator();
        }
    }
}