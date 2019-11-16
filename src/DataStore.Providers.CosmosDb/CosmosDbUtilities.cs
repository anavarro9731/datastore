using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace DataStore.Providers.CosmosDb
{
    using System;
    using System.Collections;

    internal static class CosmosDbUtilities
    {
        public static async Task ResetDatabase(CosmosSettings cosmosStoreSettings)
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
                var databases = new ArrayList();

                var iterator = client.GetDatabaseQueryIterator<DatabaseProperties>();
                do
                {
                    foreach (DatabaseProperties db in await iterator.ReadNextAsync())
                    {
                        databases.Add(db.Id);
                    }
                }
                while (iterator.HasMoreResults);

                if (databases.Contains(cosmosStoreSettings.DatabaseName))
                {
                    await client.GetDatabase(cosmosStoreSettings.DatabaseName).DeleteAsync().ConfigureAwait(false);
                }
            }

            async Task CreateDb(CosmosClient client)
            {
                var db = await client.CreateDatabaseAsync(cosmosStoreSettings.DatabaseName).ConfigureAwait(false);

                await db.Database.CreateContainerIfNotExistsAsync(
                    new ContainerProperties()
                    {
                        PartitionKeyPath = "/PartitionKey",
                        Id = cosmosStoreSettings.DatabaseName
                    }).ConfigureAwait(false);

                await Task.Delay(2000);  //the above call seems to be fire-and-forget and i need it complete reliably
            }
        }
    }
}