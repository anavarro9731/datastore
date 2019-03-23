namespace DataStore.Tests.TestHarness
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Cosmonaut;
    using DataStore.Interfaces;
    using DataStore.Interfaces.LowLevel;
    using DataStore.Models.Messages;
    using DataStore.Models.PureFunctions.Extensions;
    using DataStore.Providers.CosmosDb.ExtremeConfigAwait;
    using Microsoft.Azure.Documents;
    using Newtonsoft.Json;

    public class CosmosDbTestHarness : ITestHarness
    {
        private CosmosDbTestHarness(CosmosStoreSettings settings, IDataStore dataStore)
        {
            DataStore = dataStore;
        }

        public IDataStore DataStore { get; }

        public static async Task<ITestHarness> Create(string testName, IDataStore dataStore)
        {
            await new SynchronizationContextRemover();

            var cosmosStoreSettings = GetCosmosStoreSettings(testName);

            await ReadyTestDatabase(cosmosStoreSettings, testName).ConfigureAwait(false);

            return new CosmosDbTestHarness(cosmosStoreSettings, dataStore);
        }

        public static CosmosStoreSettings GetCosmosStoreSettings(string testName)
        {
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

            var cosmosStoreSettings = new CosmosStoreSettings(cosmosSettings.DatabaseName + testName, cosmosSettings.EndpointUrl, cosmosSettings.AuthKey);
            return cosmosStoreSettings;
        }

        public void AddToDatabase<T>(T aggregate) where T : class, IAggregate, new()
        {
            //clone aggregate to avoid modifying entries later when using inmemory db
            var newAggregate = new QueuedCreateOperation<T>(nameof(AddToDatabase), aggregate.Clone(), DataStore.DsConnection, DataStore.MessageAggregator);

            newAggregate.CommitClosure().Wait();
        }

        public IEnumerable<T> QueryDatabase<T>(Func<IQueryable<T>, IQueryable<T>> extendQueryable = null) where T : class, IAggregate, new()
        {
            var query = DataStore.DsConnection.CreateDocumentQuery<T>();
            extendQueryable?.Invoke(query);

            var results = DataStore.DsConnection.ExecuteQuery(new AggregatesQueriedOperation<T>(nameof(QueryDatabase), query.AsQueryable())).Result;
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
            public string AuthKey { get; set; }

            public string DatabaseName { get; set; }

            public string EndpointUrl { get; set; }
        }
    }
}