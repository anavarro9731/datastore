//namespace DataStore.Tests.TestHarness
//{
//    using System;
//    using System.Collections.Generic;
//    using System.Linq;
//    using CircuitBoard.MessageAggregator;
//    using CircuitBoard.Messages;
//    using global::DataStore.Impl.DocumentDb;
//    using global::DataStore.Impl.DocumentDb.Config;
//    using global::DataStore.Interfaces.LowLevel;
//    using global::DataStore.MessageAggregator;
//    using global::DataStore.Models.Messages;
//    using global::DataStore.Models.PureFunctions.Extensions;
//    using Microsoft.Azure.Documents;
//    using Microsoft.Azure.Documents.Client;

//    public class DocumentDbTestHarness : ITestHarness
//    {
//        private readonly DocumentDbRepository documentDbRepository;

//        private readonly IMessageAggregator messageAggregator = DataStoreMessageAggregator.Create();

//        private readonly DocumentDbSettings settings;

//        private DocumentDbTestHarness(DocumentDbSettings settings, DataStoreOptions dataStoreOptions)
//        {
//            this.settings = settings;
//            this.documentDbRepository = new DocumentDbRepository(settings);
//            DataStore = new DataStore(this.documentDbRepository, this.messageAggregator, dataStoreOptions);
//        }

//        public List<IMessage> AllMessages => this.messageAggregator.AllMessages.ToList();

//        public DataStore DataStore { get; }

//        public static ITestHarness Create(DocumentDbSettings dbConfig, DataStoreOptions dataStoreOptions)
//        {
//            ClearTestDatabase(dbConfig);

//            return new DocumentDbTestHarness(dbConfig, dataStoreOptions);
//        }

//        public void AddToDatabase<T>(T aggregate) where T : class, IAggregate, new()
//        {

//            //clone aggregate to avoid modifying entries later when using inmemory db
//            var newAggregate = new QueuedCreateOperation<T>(nameof(AddToDatabase), aggregate.Clone(), this.documentDbRepository, this.messageAggregator);
            
//            newAggregate.CommitClosure().Wait();
//        }

//        public IEnumerable<T> QueryDatabase<T>(Func<IQueryable<T>, IQueryable<T>> extendQueryable = null) where T : class, IAggregate, new()
//        {
//            var query = extendQueryable == null
//                            ? this.documentDbRepository.CreateDocumentQuery<T>()
//                            : extendQueryable(this.documentDbRepository.CreateDocumentQuery<T>());
//            return this.documentDbRepository.ExecuteQuery(new AggregatesQueriedOperation<T>(nameof(QueryDatabase), query.AsQueryable())).Result;
//        }

//        public async void RemoveAllCollections()
//        {
//            DocumentClient client;

//            GetDocumentClient(this.settings, out client);
//            var db = (await client.ReadDatabaseFeedAsync().ConfigureAwait(false)).Single(d => d.Id == this.settings.DatabaseName);

//            var collections = await client.ReadDocumentCollectionFeedAsync(db.CollectionsLink).ConfigureAwait(false);
//            foreach (var documentCollection in collections) await client.DeleteDocumentCollectionAsync(documentCollection.SelfLink).ConfigureAwait(false);
//        }

//        private static void ClearTestDatabase(DocumentDbSettings documentDbSettings)
//        {
//            DocumentClient client;
//            DocumentCollection collection;

//            GetDocumentClient(documentDbSettings, out client);

//            GetDocumentCollection(documentDbSettings, client, out collection);

//            if (collection != null)
//            {
//                DeleteAllDocsInCollection(client, collection);
//            }
//        }

//        private static void DeleteAllDocsInCollection(DocumentClient documentClient, DocumentCollection collection)
//        {
//            var allDocsInCollection = documentClient.CreateDocumentQuery(
//                collection.DocumentsLink,
//                new FeedOptions
//                {
//                    EnableCrossPartitionQuery = true,
//                    MaxDegreeOfParallelism = -1,
//                    MaxBufferedItemCount = -1
//                }).ToList();

//            foreach (var doc in allDocsInCollection)
//            {
//                //required if collection is partitioned
//                var requestOptions = new RequestOptions();

//                if (collection.PartitionKey.Paths.Count > 0)
//                {
//                    var key = collection.PartitionKey.Paths[0];
//                    PartitionKey partitionKey;
//                    //NOTE: these values will always be cahnged to lowercase by docdb so must make them lowercase on model
//                    if (key == "/Schema")
//                    {
//                        partitionKey = new PartitionKey(((dynamic)doc).Schema);
//                    }
//                    else if (key == "/Id")
//                    {
//                        partitionKey = new PartitionKey(doc.Id);
//                    }
//                    else
//                    {
//                        throw new Exception("Error locating partition key");
//                    }
//                    requestOptions.PartitionKey = partitionKey;
//                }

//                documentClient.DeleteDocumentAsync(doc.SelfLink, requestOptions).Wait();
//            }
//        }

//        private static void GetDocumentClient(DocumentDbSettings documentDbSettings, out DocumentClient documentClient)
//        {
//            documentClient = new DocumentDbClientFactory(documentDbSettings).GetDocumentClient();
//        }

//        private static void GetDocumentCollection(DocumentDbSettings documentDbSettings, DocumentClient documentClient, out DocumentCollection collection)
//        {
//            collection = documentClient.ReadDocumentCollectionAsync(documentDbSettings.CollectionSelfLink()).Result;
//        }
//    }
//}