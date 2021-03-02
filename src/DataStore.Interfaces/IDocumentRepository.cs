namespace DataStore.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using DataStore.Interfaces.LowLevel;
    using DataStore.Interfaces.Operations;

    public interface IDocumentRepository : IDisposable
    {
        IDatabaseSettings ConnectionSettings { get; }

        Task AddAsync<T>(IDataStoreWriteOperation<T> aggregateAdded) where T : class, IAggregate, new();

        Task<int> CountAsync<T>(IDataStoreCountFromQueryable<T> aggregatesCounted) where T : class, IAggregate, new();

        IQueryable<T> CreateQueryable<T>(
            object /* take as an object here so that you can take the more
                                                     * restrictive I..ClientSide interface at the entry point
                                                     * and dramatically cleanup the intellisense experience */
                queryOptions = null) where T : class, IAggregate, new();

        Task DeleteAsync<T>(IDataStoreWriteOperation<T> aggregateHardDeleted) where T : class, IAggregate, new();

        Task<IEnumerable<T>> ExecuteQuery<T>(IDataStoreReadFromQueryable<T> aggregatesQueried) where T : class, IAggregate, new();

        Task<T> GetItemAsync<T>(IDataStoreReadById aggregateQueriedById) where T : class, IAggregate, new();

        Task UpdateAsync<T>(IDataStoreWriteOperation<T> aggregateUpdated) where T : class, IAggregate, new();
    }
}