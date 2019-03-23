namespace DataStore.Providers.CosmosDb
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Cosmonaut;
    using Cosmonaut.Extensions;
    using DataStore.Interfaces;
    using DataStore.Interfaces.LowLevel;
    using DataStore.Providers.CosmosDb.ExtremeConfigAwait;
    using Microsoft.Azure.Documents.Linq;

    public class CosmosDbRepository : IDocumentRepository
    {

        private readonly CosmosStoreSettings cosmosStoreSettings;

        public CosmosDbRepository(CosmosStoreSettings cosmosStoreSettings)
        {
            this.cosmosStoreSettings = cosmosStoreSettings;
        }

        public async Task AddAsync<T>(IDataStoreWriteOperation<T> aggregateAdded) where T : class, IAggregate, new()
        {
            await new SynchronizationContextRemover();
            var store = new CosmosStore<T>(this.cosmosStoreSettings);
            await store.AddAsync(aggregateAdded.Model).ConfigureAwait(false);
        }

        public async Task<int> CountAsync<T>(IDataStoreCountFromQueryable<T> aggregatesCounted) where T : class, IAggregate, new()
        {

            await new SynchronizationContextRemover();
            var store = new CosmosStore<T>(this.cosmosStoreSettings);
            return await DocumentQueryable.CountAsync(store.Query().Where(aggregatesCounted.Predicate)).ConfigureAwait(false);

        }

        public IQueryable<T> CreateDocumentQuery<T>(IQueryOptions<T> queryOptions = null) where T : class, IEntity, new()
        {
            var store = new CosmosStore<T>(this.cosmosStoreSettings);
            return store.Query();
        }

        public async Task DeleteAsync<T>(IDataStoreWriteOperation<T> aggregateHardDeleted) where T : class, IAggregate, new()
        {
            await new SynchronizationContextRemover();
            var store = new CosmosStore<T>(this.cosmosStoreSettings);
            await store.RemoveByIdAsync(aggregateHardDeleted.Model.id.ToString()).ConfigureAwait(false);
        }

        public void Dispose()
        {
            //nothing to dispose
        }

        public async Task<IEnumerable<T>> ExecuteQuery<T>(IDataStoreReadFromQueryable<T> aggregatesQueried)
        {
            await new SynchronizationContextRemover();
            return await aggregatesQueried.Query.ToListAsync().ConfigureAwait(false);
        }

        public async Task<T> GetItemAsync<T>(IDataStoreReadById aggregateQueriedById) where T : class, IAggregate, new()
        {
            await new SynchronizationContextRemover();
            var store = new CosmosStore<T>(this.cosmosStoreSettings);
            return await store.FindAsync(aggregateQueriedById.Id.ToString()).ConfigureAwait(false);
        }

        public async Task UpdateAsync<T>(IDataStoreWriteOperation<T> aggregateUpdated) where T : class, IAggregate, new()
        {
            await new SynchronizationContextRemover();
            var store = new CosmosStore<T>(this.cosmosStoreSettings);
            await store.UpdateAsync(aggregateUpdated.Model).ConfigureAwait(false);
        }
    }
}