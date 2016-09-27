namespace DataStore.DataAccess.Messages.Events
{
    using Infrastructure.Messages;

    public class AggregateHardDeleted<T> : Event<T>, IDataStoreEvent
    {
        public AggregateHardDeleted(T model)
            : base(model)
        {
        }
    }
}