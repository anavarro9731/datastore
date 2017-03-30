using DataStore.Impl.DocumentDb.Config;

namespace DataStore.Impl.DocumentDb
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using Interfaces;
    using Interfaces.Events;
    using Microsoft.Azure.Documents;
    using Microsoft.Azure.Documents.Client;
    using Microsoft.Azure.Documents.Linq;
    using Models.Messages.Events;
    using ServiceApi.Interfaces.LowLevel;

    public class DocumentRepository : IDocumentRepository
    {
        private readonly DocumentDbSettings config;

        private readonly DocumentClient documentClient;

        public DocumentRepository(DocumentDbSettings config)
        {
            documentClient = new DocumentDbClientFactory(config).GetDocumentClient();
            this.config = config;
        }

        private Uri CreateDocumentSelfLinkFromId(Guid id)
        {
            if (Guid.Empty == id)
                throw new ArgumentException("Id is required for update/delete/read operation");

            var docLink = UriFactory.CreateDocumentUri(config.DatabaseName, config.CollectionSettings.CollectionName, id.ToString());
            return docLink;
        }

        #region IDocumentRepository Members

        public async Task AddAsync<T>(IDataStoreWriteEvent<T> aggregateAdded) where T : IAggregate
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
            aggregateAdded.QueryDuration = stopWatch.Elapsed;
            aggregateAdded.QueryCost = result.RequestCharge;
        }

        public IQueryable<T> CreateDocumentQuery<T>() where T : IHaveAUniqueId, IHaveSchema
        {
            var name = typeof(T).Name;
            var query = documentClient.CreateDocumentQuery<T>(config.CollectionSelfLink(),
                    new FeedOptions
                    {
                        EnableCrossPartitionQuery = config.CollectionSettings.EnableCrossParitionQueries,
                        MaxDegreeOfParallelism = -1,
                        MaxBufferedItemCount = -1
                    })
                .Where(item => item.Schema == name);
            return query;
        }

        public async Task DeleteHardAsync<T>(IDataStoreWriteEvent<T> aggregateHardDeleted) where T : IAggregate
        {
            var docLink = CreateDocumentSelfLinkFromId(aggregateHardDeleted.Model.Id);

            var stopWatch = Stopwatch.StartNew();
            var result = await DocumentDbUtils.ExecuteWithRetries(() => documentClient.DeleteDocumentAsync(docLink));
            stopWatch.Stop();
            aggregateHardDeleted.QueryCost = result.RequestCharge;
            aggregateHardDeleted.QueryDuration = stopWatch.Elapsed;
        }

        public async Task DeleteSoftAsync<T>(IDataStoreWriteEvent<T> aggregateSoftDeleted) where T : IAggregate
        {
            //HACK: this call inside the doc repository is effectively duplicate [see callers] 
            //and causes us to miss this query when profiling, arguably its cheap, but still
            //if I can determine how to create an Azure Document from T we can ditch it.
            var document = await GetItemAsync(new AggregateQueriedById(nameof(DeleteSoftAsync), aggregateSoftDeleted.Model.Id, typeof(T)));

            document.SetPropertyValue(nameof(IAggregate.Active), false);
            document.SetPropertyValue(nameof(IAggregate.Modified), DateTime.UtcNow);

            var stopWatch = Stopwatch.StartNew();
            var result = await DocumentDbUtils.ExecuteWithRetries(() => documentClient.ReplaceDocumentAsync(document.SelfLink, document));
            stopWatch.Stop();
            aggregateSoftDeleted.QueryDuration = stopWatch.Elapsed;
            aggregateSoftDeleted.QueryCost = result.RequestCharge;
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

                aggregatesQueried.QueryCost += result.RequestCharge;

                results.AddRange(result);
            }
            stopWatch.Stop();
            aggregatesQueried.QueryDuration = stopWatch.Elapsed;
            return results;
        }

        public async Task<T> GetItemAsync<T>(IDataStoreReadById aggregateQueriedById) where T : IHaveAUniqueId
        {
            var result = await GetItemAsync(aggregateQueriedById);
            return (T) (dynamic) result;
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
                aggregateQueriedById.QueryDuration = stopWatch.Elapsed;
                aggregateQueriedById.QueryCost = result.RequestCharge;

                return result.Resource;
            }
            catch (Exception e)
            {
                throw new DatabaseException($"Failed to retrieve record with id {aggregateQueriedById.Id}: {e.Message}", e);
            }
        }

        public async Task UpdateAsync<T>(IDataStoreWriteEvent<T> aggregateUpdated) where T : IAggregate
        {
            var stopWatch = Stopwatch.StartNew();
            var result =
                await
                    DocumentDbUtils.ExecuteWithRetries(
                        () => documentClient.ReplaceDocumentAsync(CreateDocumentSelfLinkFromId(aggregateUpdated.Model.Id), aggregateUpdated.Model));

            stopWatch.Stop();
            aggregateUpdated.QueryDuration = stopWatch.Elapsed;
            aggregateUpdated.QueryCost = result.RequestCharge;
        }

        public async Task<bool> Exists(IDataStoreReadById aggregateQueriedById)
        {
            var stopWatch = Stopwatch.StartNew();
            var query =
                documentClient.CreateDocumentQuery(config.CollectionSelfLink()).Where(item => item.Id == aggregateQueriedById.Id.ToString()).AsDocumentQuery();

            var results = await query.ExecuteNextAsync();

            stopWatch.Stop();
            aggregateQueriedById.QueryDuration = stopWatch.Elapsed;
            aggregateQueriedById.QueryCost = results.RequestCharge;

            return results.Count > 0;
        }

        #endregion
    }
}