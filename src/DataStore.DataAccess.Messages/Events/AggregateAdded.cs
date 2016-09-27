namespace DataStore.DataAccess.Messages.Events
{
    using Infrastructure.Messages;

    public class AggregateAdded<T> : Event<T>, IDataStoreEvent
    {
        public AggregateAdded(T model)
            : base(model)
        {
        }
    }
}