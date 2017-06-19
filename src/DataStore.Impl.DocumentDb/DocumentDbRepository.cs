using System.Net;
using DataStore.Models;
using DataStore.Models.Messages;
using Microsoft.Azure.Documents;

namespace DataStore.Impl.DocumentDb
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using Config;
    using Interfaces;
    using Interfaces.LowLevel;
    using Microsoft.Azure.Documents.Client;
    using Microsoft.Azure.Documents.Linq;
    using Models.PureFunctions.Extensions;

    public class DocumentDbRepository : IDocumentRepository
    {
        private readonly DocumentDbSettings config;

        private readonly DocumentClient documentClient;

        public DocumentDbRepository(DocumentDbSettings config)
        {
            documentClient = new DocumentDbClientFactory(config).GetDocumentClient();
            this.config = config;
        }

        private Uri CreateDocumentSelfLinkFromId(Guid id)
        {
            if (Guid.Empty == id)
                throw new ArgumentException("id is required for update/delete/read operation");

            var docLink = UriFactory.CreateDocumentUri(config.DatabaseName, config.CollectionSettings.CollectionName,
                id.ToString());
            return docLink;
        }

        #region IDocumentRepository Members

        public async Task AddAsync<T>(IDataStoreWriteOperation<T> aggregateAdded) where T : class, IAggregate, new()
        {
            if (aggregateAdded == null || aggregateAdded.Model == null)
                throw new ArgumentNullException(nameof(aggregateAdded));
                
            
            var result =
                await
                    DocumentDbUtils.ExecuteWithRetries(
                        () =>
                            documentClient.CreateDocumentAsync(
                                config.CollectionSelfLink(),
                                aggregateAdded.Model)).ConfigureAwait(false);

            aggregateAdded.StateOperationCost = result.RequestCharge;

        }

        public IQueryable<T> CreateDocumentQuery<T>() where T : class, IAggregate, new()
        {
            var name = typeof(T).FullName;
            var query = documentClient.CreateDocumentQuery<T>(config.CollectionSelfLink(),
                    new FeedOptions
                    {
                        EnableCrossPartitionQuery = config.CollectionSettings.EnableCrossParitionQueries,
                        MaxDegreeOfParallelism = -1,
                        MaxBufferedItemCount = -1
                    })
                .Where(item => item.schema == name);
            return query;
        }

        public async Task DeleteHardAsync<T>(IDataStoreWriteOperation<T> aggregateHardDeleted) where T : class, IAggregate, new()
        {
            var docLink = CreateDocumentSelfLinkFromId(aggregateHardDeleted.Model.id);

            var result = await DocumentDbUtils.ExecuteWithRetries(() => documentClient.DeleteDocumentAsync(docLink)).ConfigureAwait(false);
            aggregateHardDeleted.StateOperationCost = result.RequestCharge;
        }

        public async Task DeleteSoftAsync<T>(IDataStoreWriteOperation<T> aggregateSoftDeleted) where T : class, IAggregate, new()
        {
            //HACK: this call inside the doc repository is effectively duplicate [see callers] 
            //and causes us to miss this query when profiling, arguably its cheap, but still
            //if I can determine how to create an Azure Document from T we can ditch it.
            var document =
                await GetItemAsync(new AggregateQueriedByIdOperation(nameof(DeleteSoftAsync), aggregateSoftDeleted.Model.id, typeof(T))).ConfigureAwait(false);

            var now = DateTime.UtcNow;
            document.SetPropertyValue(nameof(IAggregate.Active), false);
            document.SetPropertyValue(nameof(IAggregate.Modified), now);
            document.SetPropertyValue(nameof(IAggregate.ModifiedAsMillisecondsEpochTime),
                now.ConvertToMillisecondsEpochTime());

            var result = await DocumentDbUtils.ExecuteWithRetries(() => documentClient.ReplaceDocumentAsync(document.SelfLink, document)).ConfigureAwait(false);

            aggregateSoftDeleted.StateOperationCost = result.RequestCharge;
        }

        public void Dispose()
        {
            documentClient.Dispose();
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

        public async Task<T> GetItemAsync<T>(IDataStoreReadById aggregateQueriedById) where T : class, IAggregate, new()
        {
            var result = await GetItemAsync(aggregateQueriedById).ConfigureAwait(false);
            return (T) result;
        }

        public async Task<dynamic> GetItemAsync(IDataStoreReadById aggregateQueriedById)
        {
            try
            {
                if (aggregateQueriedById.Id == Guid.Empty) return null; //createdocumentselflink will fail otherwise
                var result = await documentClient.ReadDocumentAsync(CreateDocumentSelfLinkFromId(aggregateQueriedById.Id))
                    .ConfigureAwait(false);
                if (result == null)
                    throw new DatabaseRecordNotFoundException(aggregateQueriedById.Id.ToString());

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
            var result =
                await
                    DocumentDbUtils.ExecuteWithRetries(
                        () =>
                            documentClient.ReplaceDocumentAsync(CreateDocumentSelfLinkFromId(aggregateUpdated.Model.id),
                                aggregateUpdated.Model)).ConfigureAwait(false);

            aggregateUpdated.StateOperationCost = result.RequestCharge;
        }

        public async Task<bool> Exists(IDataStoreReadById aggregateQueriedById)
        {
            var query =
                documentClient.CreateDocumentQuery(config.CollectionSelfLink())
                    .Where(item => item.Id == aggregateQueriedById.Id.ToString())
                    .AsDocumentQuery();

            var results = await query.ExecuteNextAsync().ConfigureAwait(false);

            aggregateQueriedById.StateOperationCost = results.RequestCharge;

            return results.Count > 0;
        }

        #endregion
    }
}