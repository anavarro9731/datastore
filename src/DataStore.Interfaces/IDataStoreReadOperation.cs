namespace DataStore.Interfaces.Events
{
    using System;
    using System.Linq;
    using ServiceApi.Interfaces.LowLevel.Messages.IntraService;

    public interface IDataStoreReadFromQueryable<T> : IDataStoreReadOperation
    {
        IQueryable<T> Query { get; set; }
    }

    public interface IDataStoreReadById : IDataStoreReadOperation
    {
        Guid Id { get; set; }
    }

    public interface IDataStoreReadOperation : IDataStoreOperation, IRequestState
    {
    }
}