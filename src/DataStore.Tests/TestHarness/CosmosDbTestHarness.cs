namespace DataStore.Tests.TestHarness
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using CircuitBoard.MessageAggregator;
    using CircuitBoard.Messages;
    using Cosmonaut;
    using global::DataStore.Interfaces.LowLevel;
    using global::DataStore.MessageAggregator;
    using global::DataStore.Models.Messages;
    using global::DataStore.Models.PureFunctions.Extensions;
    using global::DataStore.Providers.CosmosDb;
    using global::DataStore.Providers.CosmosDb.ExtremeConfigAwait;
    using Microsoft.Azure.Documents;
    using Newtonsoft.Json;

    public class CosmosDbTestHarness : ITestHarness
    {
        private readonly CosmosDbRepository cosmosDbRepository;

        private readonly IMessageAggregator messageAggregator = DataStoreMessageAggregator.Create();

        private readonly CosmosStoreSettings settings;

        private CosmosDbTestHarness(CosmosStoreSettings settings, DataStoreOptions dataStoreOptions)
        {
            this.settings = settings;
            this.cosmosDbRepository = new CosmosDbRepository(settings);
            DataStore = new DataStore(this.cosmosDbRepository, this.messageAggregator, dataStoreOptions);
        }

        public List<IMessage> AllMessages => this.messageAggregator.AllMessages.ToList();

        public DataStore DataStore { get; }

        public static async Task<ITestHarness> Create(string testName, DataStoreOptions dataStoreOptions = null)
        {
            await new SynchronizationContextRemover();

            var settingsFile = "CosmosDbSettings.json";
            /*
            Create this file in your DataStore.Tests project root and set it's build output to "copy always"
            This file should always be .gitignore(d), don't want to expose this in your repo.
            {
                "AuthKey": "<authkey>",
                "DatabaseName": "<dbname>",
                "EndpointUrl": "<endpointurl>"
            }
            */
            var location = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, settingsFile);

            var cosmosSettings = JsonConvert.DeserializeObject<CosmosSettings>(File.ReadAllText(location));
            
            var cosmosStoreSettings = new CosmosStoreSettings(cosmosSettings.DatabaseName + testName, cosmosSettings.EndpointUrl, cosmosSettings.Authkey);
            
            await ReadyTestDatabase(cosmosStoreSettings, testName).ConfigureAwait(false);

            return new CosmosDbTestHarness(cosmosStoreSettings, dataStoreOptions);
        }

        public void AddToDatabase<T>(T aggregate) where T : class, IAggregate, new()
        {
            //clone aggregate to avoid modifying entries later when using inmemory db
            var newAggregate = new QueuedCreateOperation<T>(nameof(AddToDatabase), aggregate.Clone(), this.cosmosDbRepository, this.messageAggregator);

            newAggregate.CommitClosure().Wait();
        }

        public IEnumerable<T> QueryDatabase<T>(Func<IQueryable<T>, IQueryable<T>> extendQueryable = null) where T : class, IAggregate, new()
        {
            var query = this.cosmosDbRepository.CreateDocumentQuery<T>();
            extendQueryable?.Invoke(query);

            var results = this.cosmosDbRepository.ExecuteQuery(new AggregatesQueriedOperation<T>(nameof(QueryDatabase), query.AsQueryable())).Result;
            return results;
        }

        private static async Task ReadyTestDatabase(CosmosStoreSettings cosmosStoreSettings, string testName)
        {
            {
                CreateCosmonautClient(out var cosmosClient);
                await DeleteDbIfExists(cosmosClient).ConfigureAwait(false);
                await CreateDb(cosmosClient).ConfigureAwait(false);
            }

            void CreateCosmonautClient(out CosmonautClient cosmosClient)
            {
                var pi = typeof(CosmosStoreSettings).GetProperty("AuthKey", BindingFlags.NonPublic | BindingFlags.Instance) ?? throw new NullReferenceException();
                var authKey = pi.GetValue(cosmosStoreSettings).ToString();
                cosmosClient = new CosmonautClient(cosmosStoreSettings.EndpointUrl, authKey);
            }

            async Task DeleteDbIfExists(CosmonautClient cosmonautClient)
            {
                await cosmonautClient.DeleteDatabaseAsync(cosmosStoreSettings.DatabaseName).ConfigureAwait(false);
            }

            async Task CreateDb(CosmonautClient cosmonautClient)
            {
                await cosmonautClient.CreateDatabaseAsync(
                    new Database
                    {
                        Id = cosmosStoreSettings.DatabaseName
                    }).ConfigureAwait(false);
            }
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class CosmosSettings
        {
            public string Authkey { get; set; }

            public string DatabaseName { get; set; }

            public string EndpointUrl { get; set; }
        }
    }
}