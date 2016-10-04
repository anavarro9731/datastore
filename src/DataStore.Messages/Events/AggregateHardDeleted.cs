namespace DataStore.Messages.Events
{
    public class AggregateHardDeleted<T> : Event<T>, IDataStoreEvent
    {
        public AggregateHardDeleted(T model)
            : base(model)
        {
        }
    }
}