namespace DataStore.Impl.DocumentDb
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Threading.Tasks;
    using DataStore.Impl.DocumentDb.Config;
    using DataStore.Interfaces;
    using DataStore.Interfaces.LowLevel;
    using DataStore.Models;
    using Microsoft.Azure.Documents;
    using Microsoft.Azure.Documents.Client;
    using Microsoft.Azure.Documents.Linq;

    public class DocumentDbRepository : IDocumentRepository
    {
        private readonly DocumentDbSettings config;

        private readonly DocumentClient documentClient;

        public DocumentDbRepository(DocumentDbSettings config)
        {
            this.documentClient = new DocumentDbClientFactory(config).GetDocumentClient();
            this.config = config;
        }

        public async Task AddAsync<T>(IDataStoreWriteOperation<T> aggregateAdded) where T : class, IAggregate, new()
        {
            if (aggregateAdded == null || aggregateAdded.Model == null)
            {
                throw new ArgumentNullException(nameof(aggregateAdded));
            }

            var result = await DocumentDbUtils
                               .ExecuteWithRetries(() => this.documentClient.CreateDocumentAsync(this.config.CollectionSelfLink(), aggregateAdded.Model))
                               .ConfigureAwait(false);

            aggregateAdded.StateOperationCost = result.RequestCharge;
        }

        public Task<int> CountAsync<T>(IDataStoreCountFromQueryable<T> aggregatesCounted) where T : class, IAggregate, new()
        {
            throw new NotImplementedException();
        }

        public IQueryable<T> CreateDocumentQuery<T>(IQueryOptions<T> queryOptions = null) where T : class, IEntity, new()
        {
            var name = typeof(T).FullName;
            var query = this.documentClient.CreateDocumentQuery<T>(
                this.config.CollectionSelfLink(),
                new FeedOptions
                {
                    EnableCrossPartitionQuery = this.config.CollectionSettings.EnableCrossParitionQueries,
                    MaxDegreeOfParallelism = -1,
                    MaxBufferedItemCount = -1
                }).Where(item => item.schema == name);
            return query;
        }

        public async Task DeleteAsync<T>(IDataStoreWriteOperation<T> aggregateHardDeleted) where T : class, IAggregate, new()
        {
            var docLink = CreateDocumentSelfLinkFromId(aggregateHardDeleted.Model.id);

            var result = await DocumentDbUtils.ExecuteWithRetries(() => this.documentClient.DeleteDocumentAsync(docLink)).ConfigureAwait(false);
            aggregateHardDeleted.StateOperationCost = result.RequestCharge;
        }

        public void Dispose()
        {
            this.documentClient.Dispose();
        }

        public async Task<IEnumerable<T>> ExecuteQuery<T>(IDataStoreReadFromQueryable<T> aggregatesQueried)
        {
            var results = new List<T>();

            var documentQuery = aggregatesQueried.Query.AsDocumentQuery();

            while (documentQuery.HasMoreResults)
            {
                var result = await DocumentDbUtils.ExecuteWithRetries(() => documentQuery.ExecuteNextAsync<T>()).ConfigureAwait(false);

                aggregatesQueried.StateOperationCost += result.RequestCharge;

                results.AddRange(result);
            }

            return results;
        }

        public async Task<bool> Exists(IDataStoreReadById aggregateQueriedById)
        {
            var query = this.documentClient.CreateDocumentQuery(this.config.CollectionSelfLink()).Where(item => item.Id == aggregateQueriedById.Id.ToString())
                            .AsDocumentQuery();

            var results = await query.ExecuteNextAsync().ConfigureAwait(false);

            aggregateQueriedById.StateOperationCost = results.RequestCharge;

            return results.Count > 0;
        }

        public async Task<T> GetItemAsync<T>(IDataStoreReadById aggregateQueriedById) where T : class, IAggregate, new()
        {
            var result = await GetItemAsync(aggregateQueriedById).ConfigureAwait(false);
            return (T)result;
        }

        public async Task<dynamic> GetItemAsync(IDataStoreReadById aggregateQueriedById)
        {
            try
            {
                if (aggregateQueriedById.Id == Guid.Empty) return null; //createdocumentselflink will fail otherwise
                var result = await this.documentClient.ReadDocumentAsync(CreateDocumentSelfLinkFromId(aggregateQueriedById.Id)).ConfigureAwait(false);
                if (result == null)
                {
                    throw new DatabaseRecordNotFoundException(aggregateQueriedById.Id.ToString());
                }

                aggregateQueriedById.StateOperationCost = result.RequestCharge;

                return result.Resource;
            }
            catch (DocumentClientException de)
            {
                if (de.StatusCode == HttpStatusCode.NotFound) //handle when it doesn't exists and return null
                {
                    return null;
                }

                throw new DatabaseException($"Failed to retrieve record with id {aggregateQueriedById.Id}: {de.Message}", de);
            }
            catch (Exception e)
            {
                throw new DatabaseException($"Failed to retrieve record with id {aggregateQueriedById.Id}: {e.Message}", e);
            }
        }

        public async Task UpdateAsync<T>(IDataStoreWriteOperation<T> aggregateUpdated) where T : class, IAggregate, new()
        {
            var result = await DocumentDbUtils.ExecuteWithRetries(
                             () => this.documentClient.ReplaceDocumentAsync(
                                 CreateDocumentSelfLinkFromId(aggregateUpdated.Model.id),
                                 aggregateUpdated.Model)).ConfigureAwait(false);

            aggregateUpdated.StateOperationCost = result.RequestCharge;
        }

        private Uri CreateDocumentSelfLinkFromId(Guid id)
        {
            if (Guid.Empty == id)
            {
                throw new ArgumentException("id is required for update/delete/read operation");
            }

            var docLink = UriFactory.CreateDocumentUri(this.config.DatabaseName, this.config.CollectionSettings.CollectionName, id.ToString());
            return docLink;
        }
    }
}