namespace DataStore.DataAccess.Messages.Events
{
    using Infrastructure.Messages;

    public class AggregateUpdated<T> : Event<T>, IDataStoreEvent
    {
        public AggregateUpdated(T model)
            : base(model)
        {
        }
    }
}