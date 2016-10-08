namespace DataStore.Messages.Events
{
    using System;

    public class AggregateUpdated<T> : Event<T>, IDataStoreEvent
    {
        public AggregateUpdated(T model)
            : base(model)
        {
        }

        public double QueryCost { get; set; }
        public TimeSpan QueryDuration { get; set; }
    }
}