namespace DataStore.Impl.RavenDb
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using DataStore.Interfaces;
    using DataStore.Interfaces.LowLevel;
    using DataStore.Models.PureFunctions.Extensions;
    using Raven.Client;
    using Raven.Client.Document;
    using Raven.Abstractions.Data;

    public class RavenRepository : IDocumentRepository
    {
        private IDocumentStore store;
        private readonly RavenSettings settings;

        public RavenRepository(RavenSettings settings)
        {
            this.settings = settings;

            store = new DocumentStore
            {
                Url = settings.Url,
                DefaultDatabase = settings.Database,
            }.Initialize();
        }

        public async Task AddAsync<T>(IDataStoreWriteOperation<T> aggregateAdded) where T : class, IAggregate, new()
        {
            using (IAsyncDocumentSession session = store.OpenAsyncSession())
            {
                string id = aggregateAdded.Model.id.ToString();
                await session.StoreAsync(aggregateAdded.Model, id).ConfigureAwait(false);
                await session.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        public IQueryable<T> CreateDocumentQuery<T>() where T : class, IAggregate, new()
        {
            return DoCreateDocumentQuery<T>().Result;
        }

        private async Task<IQueryable<T>> DoCreateDocumentQuery<T>() where T : class, IAggregate, new()
        {
            using (IAsyncDocumentSession session = store.OpenAsyncSession())
            {
                IList<T> resultList = await session.Query<T>().ToListAsync();
                IQueryable<T> result = resultList.AsQueryable();
                return result;
            }
        }

        public async Task DeleteHardAsync<T>(IDataStoreWriteOperation<T> aggregateHardDeleted) where T : class, IAggregate, new()
        {
            using (IAsyncDocumentSession session = store.OpenAsyncSession())
            {
                T aggregate = await session.LoadAsync<T>(aggregateHardDeleted.Model.id.ToString());
                await Task.Run(() => session.Delete<T>(aggregate));
                await session.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        public async Task DeleteSoftAsync<T>(IDataStoreWriteOperation<T> aggregateSoftDeleted) where T : class, IAggregate, new()
        {
            using (IAsyncDocumentSession session = store.OpenAsyncSession())
            {
                T aggregate = await session.LoadAsync<T>(aggregateSoftDeleted.Model.id.ToString());

                var now = DateTime.UtcNow;
                aggregateSoftDeleted.Model.Modified = now;
                aggregateSoftDeleted.Model.ModifiedAsMillisecondsEpochTime = now.ConvertToMillisecondsEpochTime();
                aggregateSoftDeleted.Model.Active = false;

                aggregateSoftDeleted.Model.CopyProperties(aggregate);

                await session.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        public void Dispose()
        {
            store.Dispose();
        }

        public Task<IEnumerable<T>> ExecuteQuery<T>(IDataStoreReadFromQueryable<T> aggregatesQueried)
        {
            var results = aggregatesQueried.Query.ToList();

            return Task.FromResult(results.AsEnumerable());
        }

        public async Task<bool> Exists(IDataStoreReadById aggregateQueriedById)
        {
            Raven.Abstractions.Data.JsonDocumentMetadata documentMetadata =
                await store.AsyncDatabaseCommands.HeadAsync(aggregateQueriedById.Id.ToString()).ConfigureAwait(false);
            return documentMetadata != null;
        }

        public Task<T> GetItemAsync<T>(IDataStoreReadById aggregateQueriedById) where T : class, IAggregate, new()
        {
            using (IAsyncDocumentSession session = store.OpenAsyncSession())
            {
                T result = session.LoadAsync<T>(aggregateQueriedById.Id.ToString()).Result;
                return Task.FromResult(result);
            }
        }

        public async Task UpdateAsync<T>(IDataStoreWriteOperation<T> aggregateUpdated) where T : class, IAggregate, new()
        {
            using (IAsyncDocumentSession session = store.OpenAsyncSession())
            {
                T aggregate = await session.LoadAsync<T>(aggregateUpdated.Model.id.ToString()).ConfigureAwait(false);
                aggregateUpdated.Model.CopyProperties(aggregate);
                await session.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        public void DropDatabase(string databaseName)
        {
            var indexDefinitions = store.DatabaseCommands.GetIndexes(0, 100);
            foreach (var indexDefinition in indexDefinitions)
            {
                store.DatabaseCommands.DeleteByIndex(indexDefinition.Name, new IndexQuery());
            }

            store.DatabaseCommands.GlobalAdmin.DeleteDatabase(databaseName, true);
            store.DatabaseCommands.GlobalAdmin.EnsureDatabaseExists(databaseName);
        }

        public void DropAllDatabases()
        {
            int skip = 0;
            List<string> databaseNames = new List<string>();
            string[] databaseNamesArray = null;
            do
            {
                databaseNamesArray = store.DatabaseCommands.GlobalAdmin.GetDatabaseNames(25, skip);
                skip += databaseNamesArray?.Length ?? 0;
                databaseNames.AddRange(databaseNamesArray);
            } while (databaseNamesArray?.Length > 0);

            databaseNames.Where(n => n.StartsWith("When")).ToList().ForEach(
                n => store.DatabaseCommands.GlobalAdmin.DeleteDatabase(n, true)
            );
        }
    }
}