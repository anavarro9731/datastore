using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace DataStore.Providers.CosmosDb
{
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

                await Task.Delay(2000);  //the above call seems to be fire-and-forget and i need it complete reliably
            }
        }
    }
}