namespace DataStore.Messages.Events
{
    public class AggregateSoftDeleted<T> : Event<T>, IDataStoreEvent
    {
        public AggregateSoftDeleted(T model)
            : base(model)
        {
        }
    }
}