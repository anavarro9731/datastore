namespace DataStore.MessageAggregator
{
    using System;
    using System.Diagnostics;
    using CircuitBoard.MessageAggregator;
    using CircuitBoard.Messages;
    using global::DataStore.Models.PureFunctions.Extensions;

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

            passTo(this.message);

            if (this.message is IStateOperation)
            {
                ((IStateOperation)this.message).StateOperationDuration = stopWatch.Elapsed;
            }
        }

        public TOut To<TOut>(Func<TMessage, TOut> passTo)
        {
            var stopWatch = new Stopwatch().Op(s => s.Start());

            var returnValue = passTo(this.message);

            if (this.message is IStateOperation)
            {
                ((IStateOperation)this.message).StateOperationDuration = stopWatch.Elapsed;
            }

            return returnValue;
        }
    }
}