namespace DataStore.MessageAggregator
{
    using System.Collections.Generic;
    using ServiceApi.Interfaces.LowLevel.MessageAggregator;
    using ServiceApi.Interfaces.LowLevel.Messages;

    public class DataStoreMessageAggregator : IMessageAggregator
    {
        private readonly List<IMessage> _allMessages = new List<IMessage>();
        public Dictionary<string, object> ReturnValues = new Dictionary<string, object>();

        public IReadOnlyList<IMessage> AllMessages => _allMessages;

        public void Collect(IMessage message)
        {
            _allMessages.Add(message);
        }

        public IPropogateMessages<TMessage> CollectAndForward<TMessage>(TMessage message) where TMessage : IGatedMessage
        {
            _allMessages.Add(message);
            return new DataStoreMessagePropogator<TMessage>(
                message,
                ReturnValues.ContainsKey(typeof(TMessage).FullName) ? ReturnValues[typeof(TMessage).FullName] : null);
        }

        public IValueReturner When<TMessage>() where TMessage : IGatedMessage
        {
            return new DataStoreValueReturner(ReturnValues, typeof(TMessage).FullName);
        }

        public static IMessageAggregator Create()
        {
            return new DataStoreMessageAggregator();
        }
    }
}