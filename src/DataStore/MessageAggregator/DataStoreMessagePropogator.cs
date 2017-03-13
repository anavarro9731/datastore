namespace DataStore.MessageAggregator
{
    using System;
    using Models.PureFunctions.Extensions;
    using ServiceApi.Interfaces.LowLevel.MessageAggregator;
    using ServiceApi.Interfaces.LowLevel.Messages;

    public class DataStoreMessagePropogator<TMessage> : IPropogateMessages<TMessage> where TMessage : IGatedMessage
    {
        private readonly TMessage message;

        private readonly object toReturn;

        internal DataStoreMessagePropogator(TMessage message, object toReturn)
        {
            this.message = message;
            this.toReturn = toReturn;
        }

        public void To(Action<TMessage> passTo)
        {
            if (this.message.GetType().InheritsOrImplements(typeof(IGatedMessage))) passTo(this.message);
        }

        public TOut To<TOut>(Func<TMessage, TOut> passTo)
        {
            if (this.message.GetType().InheritsOrImplements(typeof(IGatedMessage)))
            {
                var returnValue = passTo(this.message);

                return returnValue;
            }

            return this.toReturn != null ? (TOut)this.toReturn : default(TOut);
        }

        //public async Task<TOut> ToAsync<TOut>(Func<TMessage, Task<TOut>> forwardTo)
        //{
        //    if (this.message.GetType().InheritsOrImplements(typeof(IGatedMessage))) return await forwardTo(this.message);

        //    return this.toReturn != null ? (TOut)this.toReturn : default(TOut);
        //}

        //public Task ToAsync(Func<TMessage, Task> forwardTo)
        //{
        //    if (this.message.GetType().InheritsOrImplements(typeof(IGatedMessage))) forwardTo(this.message);

        //    return Task.FromResult(false);
        //}
    }
}