using DataStore.Interfaces.LowLevel;
using ServiceApi.Interfaces.LowLevel.Messages.IntraService;

namespace DataStore.Interfaces
{
    public interface IDataStoreWriteOperation<T> : IDataStoreWriteOperation
        where T : class, IAggregate, new()
    {
        T Model { get; set; }
    }

    public interface IDataStoreWriteOperation : IDataStoreOperation, IChangeState
    {
    }
}