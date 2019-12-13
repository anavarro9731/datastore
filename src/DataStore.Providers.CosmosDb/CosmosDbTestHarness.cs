namespace DataStore.Providers.CosmosDb
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
    using Newtonsoft.Json;

    public class CosmosDbTestHarness : ITestHarness
    {
        public static async Task<ITestHarness> Create(string testName, IDataStore dataStore)
        {
            var cosmosStoreSettings = GetCosmosStoreSettings(testName);

            await new CosmosDbUtilities().ResetDatabase(cosmosStoreSettings/**/).ConfigureAwait(false);

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

            return new CosmosSettings(cosmosSettings.AuthKey, cosmosSettings.DatabaseName + testName, cosmosSettings.EndpointUrl);
        }

        private CosmosDbTestHarness(IDataStore dataStore)
        {
            DataStore = dataStore;
        }

        public IDataStore DataStore { get; }

        public void AddToDatabase<T>(T aggregate) where T : class, IAggregate, new()
        {
            //clone aggregate to avoid modifying entries later when using inmemory db
            var newAggregate = new QueuedCreateOperation<T>(nameof(AddToDatabase), aggregate.Clone(), DataStore.DocumentRepository, DataStore.MessageAggregator);

            Task.Run(async () => await newAggregate.CommitClosure().ConfigureAwait(false)).Wait();
        }

        public IEnumerable<T> QueryDatabase<T>(Func<IQueryable<T>, IQueryable<T>> extendQueryable = null) where T : class, IAggregate, new()
        {
            var query = DataStore.DocumentRepository.CreateDocumentQuery<T>();
            var updatedQuery = extendQueryable?.Invoke(query) ?? query;

            var results = Task.Run(
                async () => await DataStore.DocumentRepository.ExecuteQuery(new AggregatesQueriedOperation<T>(nameof(QueryDatabase), updatedQuery))
                                           .ConfigureAwait(false)).Result;
            return results;
        }
    }
}