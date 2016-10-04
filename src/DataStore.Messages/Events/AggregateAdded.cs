namespace DataStore.Messages.Events
{
    public class AggregateAdded<T> : Event<T>, IDataStoreEvent
    {
        public AggregateAdded(T model)
            : base(model)
        {
        }
    }
}