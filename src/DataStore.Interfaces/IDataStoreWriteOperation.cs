namespace DataStore.Interfaces
{
    using CircuitBoard.Messages;
    using DataStore.Interfaces.LowLevel;

    public interface IDataStoreWriteOperation<T> : IDataStoreWriteOperation where T : class, IAggregate, new()
    {
        T Model { get; set; }
    }

    public interface IDataStoreWriteOperation : IDataStoreOperation, IChangeState
    {
    }

    
}