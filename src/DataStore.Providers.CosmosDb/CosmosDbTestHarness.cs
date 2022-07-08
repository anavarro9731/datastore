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
        public static async Task<ITestHarness> Create(string testName, Func<IDataStoreOptions, CosmosSettings, IDataStore> createDataStore, IDataStoreOptions options, bool useHierarchicalPartitionKey)
        {
            var cosmosStoreSettings = GetCosmosStoreSettings(testName, useHierarchicalPartitionKey);

            await new CosmosDbUtilities().ResetDatabase(cosmosStoreSettings /**/).ConfigureAwait(false);

            return new CosmosDbTestHarness(createDataStore(options, cosmosStoreSettings));
        }

        public static CosmosSettings GetCosmosStoreSettings(string testName, bool useHierarchicalPartitionKey)
        {
            var settingsFile = "CosmosDbSettings.json";
            /*
            Create this file in your DataStore.Tests project root and set it's build output to "copy always"
            This file should always be .gitignore(d), don't want to expose this in your repo.
            {
                "AuthKey": "<authkey>",
                "DatabaseName": "<dbname>",
                "EndpointUrl": "<endpointurl>",
            }
            */
            var location = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, settingsFile);

            var cosmosSettings = JsonConvert.DeserializeObject<CosmosSettings>(File.ReadAllText(location));
            
            return new CosmosSettings(cosmosSettings.AuthKey, testName, cosmosSettings.DatabaseName, cosmosSettings.EndpointUrl, useHierarchicalPartitionKey);
        }

        private CosmosDbTestHarness(IDataStore dataStore)
        {
            DataStore = dataStore;
        }

        public IDataStore DataStore { get; }

        public void AddItemDirectlyToUnderlyingDb<T>(T aggregate) where T : class, IAggregate, new()
        {
            //clone aggregate to avoid modifying entries later when using InMemoryDb
            var clone = aggregate.Clone();
            (clone as IEtagUpdated).EtagUpdated = newTag => aggregate.Etag = newTag;
            
            clone.ForcefullySetMandatoryPropertyValues(clone.ReadOnly, DataStore.DocumentRepository.UseHierarchicalPartitionKeys);

            var newAggregate = new QueuedCreateOperation<T>(
                nameof(AddItemDirectlyToUnderlyingDb),
                clone,
                DataStore.DocumentRepository,
                DataStore.MessageAggregator);

            Task.Run(async () => await newAggregate.CommitClosure().ConfigureAwait(false)).Wait();
        }

        public List<T> QueryUnderlyingDbDirectly<T>(Func<IQueryable<T>, IQueryable<T>> extendQueryable = null)
            where T : class, IAggregate, new()
        {
            var query = DataStore.DocumentRepository.CreateQueryable<T>();
            var updatedQuery = extendQueryable?.Invoke(query) ?? query;

            var results = Task.Run(
                async () => await DataStore.DocumentRepository.ExecuteQuery(
                                new AggregatesQueriedOperation<T>(
                                    nameof(QueryUnderlyingDbDirectly),
                                    updatedQuery)).ConfigureAwait(false)).Result;
            return results.ToList();
        }
    }
}
