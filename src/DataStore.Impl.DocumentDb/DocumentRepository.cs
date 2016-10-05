namespace DataStore.DataAccess.Impl.DocumentDb
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Interfaces;
    using Messages.Events;
    using Microsoft.Azure.Documents;
    using Microsoft.Azure.Documents.Client;
    using Microsoft.Azure.Documents.Linq;
    using Models.Config;

    public class DocumentRepository : IDocumentRepository
    {
        private readonly DocumentDbSettings _config;

        private readonly DocumentClient _documentClient;

        public DocumentRepository(DocumentDbSettings config)
        {
            _documentClient = new DocumentDbClientFactory(config).GetDocumentClient();
            this._config = config;
        }

        #region IDocumentRepository Members

        public async Task<T> AddAsync<T>(AggregateAdded<T> aggregateAdded) where T : IHaveAUniqueId
        {
            if (aggregateAdded == null || aggregateAdded.Model == null)
            {
                throw new ArgumentNullException(nameof(aggregateAdded));
            }

            var disableAutoIdGeneration = aggregateAdded.Model.id != Guid.Empty;

            var result =
                await
                    DocumentDbUtils.ExecuteWithRetries(
                        () =>
                            _documentClient.CreateDocumentAsync(
                                _config.DatabaseSelfLink(),
                                aggregateAdded.Model,
                                disableAutomaticIdGeneration: disableAutoIdGeneration));

            return (T) (dynamic) result.Resource;
        }

        public IQueryable<T> CreateDocumentQuery<T>() where T : IHaveAUniqueId, IHaveSchema
        {
            var name = typeof(T).Name;
            var query = _documentClient.CreateDocumentQuery<T>(_config.DatabaseSelfLink()).Where(item => item.Schema == name);
            return query;
        }

        public async Task<T> DeleteHardAsync<T>(AggregateHardDeleted<T> aggregateHardDeleted) where T : IHaveAUniqueId
        {
            var docLink = CreateDocumentSelfLinkFromId(aggregateHardDeleted.Model.id);
            return (T) (dynamic) (await DocumentDbUtils.ExecuteWithRetries(() => _documentClient.DeleteDocumentAsync(docLink))).Resource;
        }

        public async Task<T> DeleteSoftAsync<T>(AggregateSoftDeleted<T> aggregateSoftDeleted) where T : IHaveAUniqueId
        {
            var document = await GetItemAsync(aggregateSoftDeleted.Model.id);
            document.SetPropertyValue(nameof(IAggregate.Active), false);
            return (T) (dynamic) (await DocumentDbUtils.ExecuteWithRetries(() => _documentClient.ReplaceDocumentAsync(document.SelfLink, document))).Resource;
        }

        public void Dispose()
        {
            _documentClient.Dispose();
        }

        public async Task<IEnumerable<T>> ExecuteQuery<T>(IQueryable<T> query) where T : IHaveAUniqueId
        {
            var results = new List<T>();

            var documentQuery = query.AsDocumentQuery();
            while (documentQuery.HasMoreResults)
            {
                results.AddRange(
                    await DocumentDbUtils.ExecuteWithRetries(() => documentQuery.ExecuteNextAsync<T>()).ConfigureAwait(false));
            }

            return results;
        }

        public async Task<bool> Exists(Guid id)
        {
            var query = _documentClient.CreateDocumentQuery(_config.DatabaseSelfLink()).Where(item => item.Id == id.ToString()).AsDocumentQuery();
            var results = await query.ExecuteNextAsync();
            return results.Count > 0;
        }

        public async Task<T> GetItemAsync<T>(Guid id) where T : IHaveAUniqueId
        {
            var result = await GetItemAsync(id);
            return (T) (dynamic) result;
        }

        public async Task<Document> GetItemAsync(Guid id)
        {
            try
            {
                var result = await _documentClient.ReadDocumentAsync(CreateDocumentSelfLinkFromId(id));
                if (result == null)
                {
                    throw new DatabaseRecordNotFoundException(id.ToString());
                }

                return result.Resource;
            }
            catch (Exception e)
            {
                throw new DatabaseException($"Failed to retrieve record with id {id}: {e.Message}", e);
            }
        }

        public async Task<T> UpdateAsync<T>(AggregateUpdated<T> aggregateUpdated) where T : IHaveAUniqueId
        {
            return
                (T) (dynamic) (await
                    DocumentDbUtils.ExecuteWithRetries(
                        () =>
                            _documentClient.ReplaceDocumentAsync(
                                CreateDocumentSelfLinkFromId(aggregateUpdated.Model.id),
                                aggregateUpdated.Model))).Resource;
        }

        #endregion

        private Uri CreateDocumentSelfLinkFromId(Guid id)
        {
            if (Guid.Empty == id)
            {
                throw new ArgumentException("Id is required for update/delete/read operation");
            }

            var docLink = UriFactory.CreateDocumentUri(_config.DatabaseName, _config.DefaultCollectionName, id.ToString());
            return docLink;
        }
    }
}