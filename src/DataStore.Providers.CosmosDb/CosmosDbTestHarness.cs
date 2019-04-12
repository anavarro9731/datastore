namespace DataStore.Tests.TestHarness
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using DataStore.Interfaces;
    using DataStore.Interfaces.LowLevel;
    using DataStore.Models.Messages;
    using DataStore.Models.PureFunctions.Extensions;
    using DataStore.Providers.CosmosDb;
    using Microsoft.Azure.Cosmos;
    using Newtonsoft.Json;

    public class CosmosDbTestHarness : ITestHarness
    {
        private CosmosDbTestHarness(IDataStore dataStore)
        {
            DataStore = dataStore;
        }

        public IDataStore DataStore { get; }

        public static async Task<ITestHarness> Create(string testName, IDataStore dataStore)
        {
            var cosmosStoreSettings = GetCosmosStoreSettings(testName);

            await ReadyTestDatabase(cosmosStoreSettings, testName).ConfigureAwait(false);

            return new CosmosDbTestHarness(dataStore);
        }

        public static CosmosSettings GetCosmosStoreSettings(string testName)
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

            var cosmosStoreSettings = new CosmosSettings(cosmosSettings.AuthKey, cosmosSettings.DatabaseName + testName, cosmosSettings.EndpointUrl);
            return cosmosStoreSettings;
        }

        public void AddToDatabase<T>(T aggregate) where T : class, IAggregate, new()
        {
            //clone aggregate to avoid modifying entries later when using inmemory db
            var newAggregate = new QueuedCreateOperation<T>(nameof(AddToDatabase), aggregate.Clone(), DataStore.DsConnection, DataStore.MessageAggregator);
            
            Task.Run(async () => await newAggregate.CommitClosure().ConfigureAwait(false)).Wait();
            
        }

        public IEnumerable<T> QueryDatabase<T>(Func<IQueryable<T>, IQueryable<T>> extendQueryable = null) where T : class, IAggregate, new()
        {
            var query = DataStore.DsConnection.CreateDocumentQuery<T>();
            var updatedQuery = extendQueryable?.Invoke(query) ?? query;

            var results = 
                Task.Run(async () => await DataStore.DsConnection.ExecuteQuery(new AggregatesQueriedOperation<T>(nameof(QueryDatabase), updatedQuery)).ConfigureAwait(false)).Result;
            return results;
        }

        private static async Task ReadyTestDatabase(CosmosSettings cosmosStoreSettings, string testName)
        {
            {
                CreateClient(out var cosmosClient);
                await DeleteDbIfExists(cosmosClient).ConfigureAwait(false);
                await CreateDb(cosmosClient).ConfigureAwait(false);
            }

            void CreateClient(out CosmosClient client)
            {
                client = new CosmosClient(cosmosStoreSettings.EndpointUrl, cosmosStoreSettings.AuthKey);
            }

            async Task DeleteDbIfExists(CosmosClient client)
            {
                await client.Databases[cosmosStoreSettings.DatabaseName].DeleteAsync().ConfigureAwait(false);
            }

            async Task CreateDb(CosmosClient client)
            {
                var db = await client.Databases.CreateDatabaseAsync(cosmosStoreSettings.DatabaseName).ConfigureAwait(false);

                await db.Database.Containers.CreateContainerIfNotExistsAsync(
                    new CosmosContainerSettings
                    {
                        Id = cosmosStoreSettings.DatabaseName,
                        PartitionKey = new PartitionKeyDefinition
                        {
                            Paths =
                            {
                                "/PartitionKey"
                            }
                        }
                    }).ConfigureAwait(false);

                await Task.Delay(2000);  //this call seems to be fire-and-forget and i need it complete reliably
            }
        }
    }
}