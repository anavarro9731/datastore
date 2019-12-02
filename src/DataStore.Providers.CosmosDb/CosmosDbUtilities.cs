namespace DataStore.Providers.CosmosDb
{
    using System.Collections;
    using System.Threading.Tasks;
    using Microsoft.Azure.Cosmos;

    internal static class CosmosDbUtilities
    {
        public static async Task CreateDatabase(CosmosSettings cosmosStoreSettings)
        {
            {
                CreateClient(cosmosStoreSettings, out var cosmosClient);
                await CreateDb(cosmosClient, cosmosStoreSettings).ConfigureAwait(false);
            }
        }

        public static async Task ResetDatabase(CosmosSettings cosmosStoreSettings)
        {
            {
                CreateClient(cosmosStoreSettings, out var cosmosClient);
                await DeleteDbIfExists(cosmosClient).ConfigureAwait(false);
                await CreateDb(cosmosClient, cosmosStoreSettings).ConfigureAwait(false);
            }

            async Task DeleteDbIfExists(CosmosClient client)
            {
                var databases = new ArrayList();

                var iterator = client.GetDatabaseQueryIterator<DatabaseProperties>();
                do
                {
                    foreach (var db in await iterator.ReadNextAsync()) databases.Add(db.Id);
                }
                while (iterator.HasMoreResults);

                if (databases.Contains(cosmosStoreSettings.DatabaseName))
                {
                    await client.GetDatabase(cosmosStoreSettings.DatabaseName).DeleteAsync().ConfigureAwait(false);
                }
            }
        }

        private static void CreateClient(CosmosSettings cosmosStoreSettings, out CosmosClient client)
        {
            client = new CosmosClient(cosmosStoreSettings.EndpointUrl, cosmosStoreSettings.AuthKey);
        }

        private static async Task CreateDb(CosmosClient client, CosmosSettings cosmosStoreSettings)
        {
            var db = await client.CreateDatabaseAsync(cosmosStoreSettings.DatabaseName).ConfigureAwait(false);

            await db.Database.CreateContainerIfNotExistsAsync(
                new ContainerProperties
                {
                    PartitionKeyPath = "/PartitionKey",
                    Id = cosmosStoreSettings.DatabaseName
                }).ConfigureAwait(false);

            await Task.Delay(2000); //the above call seems to be fire-and-forget and i need it complete reliably
        }
    }
}