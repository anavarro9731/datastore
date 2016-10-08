namespace DataStore.Messages.Events
{
    using System;

    public class AggregateAdded<T> : Event<T>, IDataStoreEvent
    {
        public AggregateAdded(T model)
            : base(model)
        {
        }

        public double QueryCost { get; set; }
        public TimeSpan QueryDuration { get; set; }
    }
}