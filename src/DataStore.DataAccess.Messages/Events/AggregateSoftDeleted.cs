namespace DataAccess.Messages.Events
{
    using Infrastructure.Messages;

    public class AggregateSoftDeleted<T> : Event<T>, IDataStoreEvent
    {
        public AggregateSoftDeleted(T model)
            : base(model)
        {
        }
    }
}