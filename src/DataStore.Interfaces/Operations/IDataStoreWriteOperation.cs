namespace DataStore.Interfaces.Operations
{
    #region

    using System.Collections.Generic;
    using CircuitBoard.Messages;
    using DataStore.Interfaces.LowLevel;

    #endregion

    public interface IDataStoreWriteOperation<T> : IDataStoreWriteOperation where T : class, IAggregate, new()
    {
        new T Model { get; set; }
    }

    public interface IDataStoreWriteOperation : IDataStoreOperation, IChangeState
    {
        List<Aggregate.AggregateVersionInfo> GetHistoryItems { get; }

        IAggregate Model { set; }
    }
}