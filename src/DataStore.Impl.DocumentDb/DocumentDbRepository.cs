using DataStore.Models;
using DataStore.Models.Messages;

namespace DataStore.Impl.DocumentDb
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using Config;
    using Interfaces;
    using Interfaces.Events;
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
                
            var stopWatch = Stopwatch.StartNew();
            var result =
                await
                    DocumentDbUtils.ExecuteWithRetries(
                        () =>
                            documentClient.CreateDocumentAsync(
                                config.CollectionSelfLink(),
                                aggregateAdded.Model));
            stopWatch.Stop();
            aggregateAdded.StateOperationDuration = stopWatch.Elapsed;
            aggregateAdded.StateOperationCost = result.RequestCharge;
        }

        public IQueryable<T> CreateDocumentQuery<T>() where T : IHaveAUniqueId, IHaveSchema
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

            var stopWatch = Stopwatch.StartNew();
            var result = await DocumentDbUtils.ExecuteWithRetries(() => documentClient.DeleteDocumentAsync(docLink));
            stopWatch.Stop();
            aggregateHardDeleted.StateOperationCost = result.RequestCharge;
            aggregateHardDeleted.StateOperationDuration = stopWatch.Elapsed;
        }

        public async Task DeleteSoftAsync<T>(IDataStoreWriteOperation<T> aggregateSoftDeleted) where T : class, IAggregate, new()
        {
            //HACK: this call inside the doc repository is effectively duplicate [see callers] 
            //and causes us to miss this query when profiling, arguably its cheap, but still
            //if I can determine how to create an Azure Document from T we can ditch it.
            var document =
                await GetItemAsync(new AggregateQueriedByIdOperation(nameof(DeleteSoftAsync), aggregateSoftDeleted.Model.id, typeof(T)));

            var now = DateTime.UtcNow;
            document.SetPropertyValue(nameof(IAggregate.Active), false);
            document.SetPropertyValue(nameof(IAggregate.Modified), now);
            document.SetPropertyValue(nameof(IAggregate.ModifiedAsMillisecondsEpochTime),
                now.ConvertToMillisecondsEpochTime());

            var stopWatch = Stopwatch.StartNew();
            var result =
                await DocumentDbUtils.ExecuteWithRetries(() => documentClient.ReplaceDocumentAsync(document.SelfLink, document));
            stopWatch.Stop();
            aggregateSoftDeleted.StateOperationDuration = stopWatch.Elapsed;
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
            var stopWatch = Stopwatch.StartNew();
            while (documentQuery.HasMoreResults)
            {
                var result = await DocumentDbUtils.ExecuteWithRetries(() => documentQuery.ExecuteNextAsync<T>());

                aggregatesQueried.StateOperationCost += result.RequestCharge;

                results.AddRange(result);
            }
            stopWatch.Stop();
            aggregatesQueried.StateOperationDuration = stopWatch.Elapsed;
            return results;
        }

        public async Task<T> GetItemAsync<T>(IDataStoreReadById aggregateQueriedById) where T : IHaveAUniqueId
        {
            var result = await GetItemAsync(aggregateQueriedById);
            return (T) result;
        }

        public async Task<dynamic> GetItemAsync(IDataStoreReadById aggregateQueriedById)
        {
            try
            {
                var stopWatch = Stopwatch.StartNew();
                var result = await documentClient.ReadDocumentAsync(CreateDocumentSelfLinkFromId(aggregateQueriedById.Id));
                if (result == null)
                    throw new DatabaseRecordNotFoundException(aggregateQueriedById.Id.ToString());
                stopWatch.Stop();
                aggregateQueriedById.StateOperationDuration = stopWatch.Elapsed;
                aggregateQueriedById.StateOperationCost = result.RequestCharge;

                return result.Resource;
            }
            catch (Exception e)
            {
                throw new DatabaseException($"Failed to retrieve record with id {aggregateQueriedById.Id}: {e.Message}", e);
            }
        }

        public async Task UpdateAsync<T>(IDataStoreWriteOperation<T> aggregateUpdated) where T : class, IAggregate, new()
        {
            var stopWatch = Stopwatch.StartNew();
            var result =
                await
                    DocumentDbUtils.ExecuteWithRetries(
                        () =>
                            documentClient.ReplaceDocumentAsync(CreateDocumentSelfLinkFromId(aggregateUpdated.Model.id),
                                aggregateUpdated.Model));

            stopWatch.Stop();
            aggregateUpdated.StateOperationDuration = stopWatch.Elapsed;
            aggregateUpdated.StateOperationCost = result.RequestCharge;
        }

        public async Task<bool> Exists(IDataStoreReadById aggregateQueriedById)
        {
            var stopWatch = Stopwatch.StartNew();
            var query =
                documentClient.CreateDocumentQuery(config.CollectionSelfLink())
                    .Where(item => item.Id == aggregateQueriedById.Id.ToString())
                    .AsDocumentQuery();

            var results = await query.ExecuteNextAsync();

            stopWatch.Stop();
            aggregateQueriedById.StateOperationDuration = stopWatch.Elapsed;
            aggregateQueriedById.StateOperationCost = results.RequestCharge;

            return results.Count > 0;
        }

        #endregion
    }
}