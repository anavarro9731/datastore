using DataStore.Impl.DocumentDb.Config;

namespace DataStore.Tests.TestHarness
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using global::DataStore;
    using global::DataStore.Impl.DocumentDb;
    using global::DataStore.Interfaces;
    using global::DataStore.Interfaces.Events;
    using global::DataStore.MessageAggregator;
    using global::DataStore.Models.Messages.Events;
    using Interfaces.LowLevel;
    using Microsoft.Azure.Documents;
    using Microsoft.Azure.Documents.Client;
    using ServiceApi.Interfaces.LowLevel;
    using ServiceApi.Interfaces.LowLevel.MessageAggregator;

    public class RemoteTestHarness : ITestHarness
    {
        private readonly IMessageAggregator _eventAggregator = DataStoreMessageAggregator.Create();
        private readonly DocumentRepository documentRepository;

        private RemoteTestHarness(DocumentRepository documentRepository)
        {
            this.documentRepository = documentRepository;
            DataStore = new DataStore(this.documentRepository, _eventAggregator);
        }

        public DataStore DataStore { get; }
        public List<IDataStoreEvent> Events => _eventAggregator.AllMessages.OfType<IDataStoreEvent>().ToList();

        public async Task AddToDatabase<T>(T aggregate) where T : IAggregate
        {
            var newAggregate = new AggregateAdded<T>(nameof(AddToDatabase), aggregate, documentRepository);
            await newAggregate.CommitClosure();
        }

        public async Task<IEnumerable<T>> QueryDatabase<T>(Func<IQueryable<T>, IQueryable<T>> extendQueryable = null) where T : IHaveSchema, IHaveAUniqueId
        {
            var query = extendQueryable == null ? documentRepository.CreateDocumentQuery<T>() : extendQueryable(documentRepository.CreateDocumentQuery<T>());
            return await documentRepository.ExecuteQuery(new AggregatesQueried<T>(nameof(QueryDatabase), query.AsQueryable()));
        }

        public static ITestHarness Create(DocumentDbSettings dbConfig)
        {
            ClearTestDatabase(dbConfig);

            return new RemoteTestHarness(new DocumentRepository(dbConfig));
        }

        private static void ClearTestDatabase(DocumentDbSettings documentDbSettings)
        {
            DocumentClient client;
            DocumentCollection collection;

            GetDocumentClient(documentDbSettings, out client);

            GetDocumentCollection(documentDbSettings, client, out collection);

            if (collection != null)
                DeleteAllDocsInCollection(client, collection);
        }

        private static void DeleteAllDocsInCollection(DocumentClient documentClient, DocumentCollection collection)
        {
            var allDocsInCollection = documentClient.CreateDocumentQuery(collection.DocumentsLink,
                new FeedOptions {EnableCrossPartitionQuery = true, MaxDegreeOfParallelism = -1, MaxBufferedItemCount = -1}).ToList();

            foreach (var doc in allDocsInCollection)
            {
                //required if collection is partitioned
                var requestOptions = new RequestOptions();

                if (collection.PartitionKey.Paths.Count > 0)
                {
                    var key = collection.PartitionKey.Paths[0];
                    PartitionKey partitionKey;
                    //NOTE: these values willalways be cahnged to lowercase by docdb so must make them lowercase on model
                    if (key == "/schema") partitionKey = new PartitionKey(((dynamic) doc).schema);
                    else if (key == "/id") partitionKey = new PartitionKey(doc.Id);
                    else throw new Exception("Error locating partition key");
                    requestOptions.PartitionKey = partitionKey;
                }

                documentClient.DeleteDocumentAsync(doc.SelfLink, requestOptions).Wait();
            }
        }

        private static void GetDocumentCollection(DocumentDbSettings documentDbSettings, DocumentClient documentClient, out DocumentCollection collection)
        {
            collection =
                documentClient.ReadDocumentCollectionAsync(documentDbSettings.CollectionSelfLink()).Result;
        }

        private static void GetDocumentClient(DocumentDbSettings documentDbSettings, out DocumentClient documentClient)
        {
            documentClient = new DocumentDbClientFactory(documentDbSettings).GetDocumentClient();
        }
    }
}