namespace DataStore.DataAccess.Interfaces.Events
{
    using System;
    using System.Threading.Tasks;

    public interface IDataStoreWriteEvent<T> : IDataStoreWriteEvent, IDataStoreEvent where T : IAggregate
    {
        T Model { get; }
    }

    public interface IDataStoreWriteEvent
    {
        Func<Task> CommitClosure { get; set; }

        bool Committed { get; set; }

        Guid AggregateId { get; }
    }
}