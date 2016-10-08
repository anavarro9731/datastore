namespace DataStore.Messages.Events
{
    using System;

    public class AggregateHardDeleted<T> : Event<T>, IDataStoreEvent
    {
        public AggregateHardDeleted(T model)
            : base(model)
        {
        }

        public double QueryCost { get; set; }
        public TimeSpan QueryDuration { get; set; }
    }
}