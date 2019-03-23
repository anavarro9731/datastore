namespace DataStore.Providers.CosmosDb
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Cosmonaut;
    using Cosmonaut.Extensions;
    using DataStore.Interfaces;
    using DataStore.Interfaces.LowLevel;
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
            var store = new CosmosStore<T>(this.cosmosStoreSettings);
            await store.AddAsync(aggregateAdded.Model);
        }

        public async Task<int> CountAsync<T>(IDataStoreCountFromQueryable<T> aggregatesCounted) where T : class, IAggregate, new()
        {
            var store = new CosmosStore<T>(this.cosmosStoreSettings);
            return await DocumentQueryable.CountAsync(store.Query().Where(aggregatesCounted.Predicate));
        }

        public IQueryable<T> CreateDocumentQuery<T>(IQueryOptions<T> queryOptions = null) where T : class, IEntity, new()
        {
            var store = new CosmosStore<T>(this.cosmosStoreSettings);
            return store.Query();
        }

        public async Task DeleteAsync<T>(IDataStoreWriteOperation<T> aggregateHardDeleted) where T : class, IAggregate, new()
        {
            var store = new CosmosStore<T>(this.cosmosStoreSettings);
            await store.RemoveByIdAsync(aggregateHardDeleted.Model.Id.ToString());
        }

        public void Dispose()
        {
            //nothing to dispose
        }

        public async Task<IEnumerable<T>> ExecuteQuery<T>(IDataStoreReadFromQueryable<T> aggregatesQueried)
        {
            return await aggregatesQueried.Query.ToListAsync();
        }

        public async Task<T> GetItemAsync<T>(IDataStoreReadById aggregateQueriedById) where T : class, IAggregate, new()
        {
            var store = new CosmosStore<T>(this.cosmosStoreSettings);
            return await store.FindAsync(aggregateQueriedById.Id.ToString());
        }

        public async Task UpdateAsync<T>(IDataStoreWriteOperation<T> aggregateUpdated) where T : class, IAggregate, new()
        {
            var store = new CosmosStore<T>(this.cosmosStoreSettings);
            await store.UpdateAsync(aggregateUpdated.Model);
        }
    }
}