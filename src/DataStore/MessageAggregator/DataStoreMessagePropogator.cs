using System;
using System.Diagnostics;
using DataStore.Models.PureFunctions.Extensions;
using ServiceApi.Interfaces.LowLevel.MessageAggregator;
using ServiceApi.Interfaces.LowLevel.Messages;
using ServiceApi.Interfaces.LowLevel.Messages.IntraService;

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
            var stopWatch = new Stopwatch().Op(s => s.Start());

            passTo(message);

            if (message is IStateOperation)
            {
                ((IStateOperation) message).StateOperationDuration = stopWatch.Elapsed;
            }
        }

        public TOut To<TOut>(Func<TMessage, TOut> passTo)
        {
            var stopWatch = new Stopwatch().Op(s => s.Start());

            var returnValue = passTo(message);

            if (message is IStateOperation)
            {
                ((IStateOperation)message).StateOperationDuration = stopWatch.Elapsed;
            }

            return returnValue;
        }
    }
}