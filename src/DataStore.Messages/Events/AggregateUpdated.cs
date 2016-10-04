namespace DataStore.Messages.Events
{
    public class AggregateUpdated<T> : Event<T>, IDataStoreEvent
    {
        public AggregateUpdated(T model)
            : base(model)
        {
        }
    }
}