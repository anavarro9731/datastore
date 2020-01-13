namespace DataStore.Providers.CosmosDb
{
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;
    using DataStore.Interfaces;
    using DataStore.Models.PureFunctions.Extensions;
    using Microsoft.Azure.Cosmos;

    public class CosmosDbUtilities : IDatabaseUtilities
    {
        private static readonly AsyncLocal<Action<string>> writeLine = new AsyncLocal<Action<string>>().Op(al => al.Value = (msg) => { });

        public static Action<string> WriteLine { 
            get => writeLine.Value; 
            set => writeLine.Value = message => value?.Invoke(message);
        }

        private static void CreateClient(CosmosSettings cosmosStoreSettings, out CosmosClient client)
        {
            client = new CosmosClient(cosmosStoreSettings.EndpointUrl, cosmosStoreSettings.AuthKey);
        }

        private static async Task CreateDb(CosmosClient client, CosmosSettings cosmosStoreSettings)
        {
            var databases = await ListDatabases(client);

            if (!databases.Contains(cosmosStoreSettings.DatabaseName))
            {
                var db = await client.CreateDatabaseAsync(cosmosStoreSettings.DatabaseName).ConfigureAwait(false);

                var response = await db.Database.CreateContainerIfNotExistsAsync(
                                   new ContainerProperties
                                   {
                                       PartitionKeyPath = "/PartitionKey",
                                       Id = cosmosStoreSettings.DatabaseName
                                   }).ConfigureAwait(false);

                await Task.Delay(5000); //the above call seems to be fire-and-forget and i need it complete reliably
            }
        }

        private static async Task<ArrayList> ListDatabases(CosmosClient client)
        {
            var databases = new ArrayList();

            var iterator = client.GetDatabaseQueryIterator<DatabaseProperties>();
            do
            {
                foreach (var db in await iterator.ReadNextAsync()) databases.Add(db.Id);
            }
            while (iterator.HasMoreResults);

            return databases;
        }

        public async Task CreateDatabaseIfNotExists(IDatabaseSettings cosmosStoreSettings)
        {
            {
                CreateClient((CosmosSettings)cosmosStoreSettings, out var cosmosClient);
                await CreateDb(cosmosClient, (CosmosSettings)cosmosStoreSettings).ConfigureAwait(false);
            }
        }

        public async Task ResetDatabase(IDatabaseSettings cosmosStoreSettings)
        {
            {
                var cosmosSettings = (CosmosSettings)cosmosStoreSettings;

                CreateClient(cosmosSettings, out var cosmosClient);
                await DeleteDbIfExists(cosmosClient, cosmosSettings).ConfigureAwait(false);
                await CreateDb(cosmosClient, cosmosSettings).ConfigureAwait(false);
            }

            async Task DeleteDbIfExists(CosmosClient client, CosmosSettings cosmosSettings)
            {
                var databases = await ListDatabases(client);

                if (databases.Contains(cosmosSettings.DatabaseName))
                {
                    await client.GetDatabase(cosmosSettings.DatabaseName).DeleteAsync().ConfigureAwait(false);
                }
            }
        }
    }
}