using System;
using ServiceApi.Interfaces.LowLevel.MessageAggregator;
using ServiceApi.Interfaces.LowLevel.Messages;

namespace DataStore.MessageAggregator
{
    public class DataStoreMessagePropogator<TMessage> : IPropogateMessages<TMessage> where TMessage : IMessage
    {
        private readonly TMessage message;

        internal DataStoreMessagePropogator(TMessage message)
        {
            this.message = message;
        }

        public void To(Action<TMessage> passTo)
        {
            passTo(message);
        }

        public TOut To<TOut>(Func<TMessage, TOut> passTo)
        {
            var returnValue = passTo(message);

            return returnValue;
        }
    }
}