namespace DataStore.Interfaces
{
    #region

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using DataStore.Interfaces.LowLevel;
    using DataStore.Interfaces.Operations;
    using DataStore.Interfaces.Options;
    using DataStore.Interfaces.Options.LibrarySide.Interfaces;

    #endregion

    public interface IDocumentRepository : IDisposable
    {
        bool UseHierarchicalPartitionKeys { get; }

        IDatabaseSettings ConnectionSettings { get; }

        Task AddAsync<T>(IDataStoreWriteOperation<T> aggregateAdded) where T : class, IAggregate, new();

        Task<int> CountAsync<T>(IDataStoreCountFromQueryable<T> aggregatesCounted) where T : class, IAggregate, new();

        IQueryable<T> CreateQueryable<T>(
            IOptionsLibrarySide /* take as an IQueryOptions here so that you can take the more
                                                     * restrictive I..ClientSide interface at the entry point
                                                     * and dramatically cleanup the intellisense experience */
                queryOptions) where T : class, IAggregate, new();

        Task DeleteAsync<T>(IDataStoreWriteOperation<T> aggregateHardDeleted) where T : class, IAggregate, new();

        Task<IEnumerable<T>> ExecuteQuery<T>(IDataStoreReadFromQueryable<T> aggregatesQueried) where T : class, IAggregate, new();

        Task<T> GetItemAsync<T>(IDataStoreReadByIdOperation aggregateQueriedByIdOperation) where T : class, IAggregate, new();

        Task UpdateAsync<T>(IDataStoreWriteOperation<T> aggregateUpdated) where T : class, IAggregate, new();
    }
}