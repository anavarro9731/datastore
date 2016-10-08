namespace DataStore.Messages.Events
{
    using System;

    public class AggregateSoftDeleted<T> : Event<T>, IDataStoreEvent
    {
        public AggregateSoftDeleted(T model)
            : base(model)
        {
        }

        public double QueryCost { get; set; }
        public TimeSpan QueryDuration { get; set; }
    }
}