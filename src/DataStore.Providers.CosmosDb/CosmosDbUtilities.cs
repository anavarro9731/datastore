namespace DataStore.Providers.CosmosDb
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using DataStore.Interfaces;
    using DataStore.Interfaces.LowLevel;
    using Microsoft.Azure.Cosmos;

    public class CosmosDbUtilities : IDatabaseUtilities
    {
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
                var databases = await ListDatabases(client).ConfigureAwait(false);

                if (databases.Contains(cosmosSettings.DatabaseName))
                {
                    var db = client.GetDatabase(cosmosSettings.DatabaseName);
                    var containers = await ListContainers(db).ConfigureAwait(false);
                    if (containers.Contains(cosmosSettings.ContainerName))
                    {
                        await db.GetContainer(cosmosSettings.ContainerName).DeleteContainerAsync().ConfigureAwait(false);
                    }
                }
            }
        }

        internal static void CreateClient(CosmosSettings cosmosSettings, out CosmosClient client)
        {
            client = new CosmosClient(cosmosSettings.EndpointUrl, cosmosSettings.AuthKey, cosmosSettings.ClientOptions);
        }

        private static async Task CreateDb(CosmosClient client, CosmosSettings cosmosStoreSettings)
        {
            var databases = await ListDatabases(client).ConfigureAwait(false);

            Database db;

            if (!databases.Contains(cosmosStoreSettings.DatabaseName))
            {
                db = await client.CreateDatabaseAsync(cosmosStoreSettings.DatabaseName, ThroughputProperties.CreateAutoscaleThroughput(400)).ConfigureAwait(false);
            }
            else
            {
                db = client.GetDatabase(cosmosStoreSettings.DatabaseName);
            }

            await CreateContainerIfNotExists(cosmosStoreSettings, db).ConfigureAwait(false);

            await Task.Delay(5000).ConfigureAwait(false);

            //the above call seems to be fire-and-forget and i need it complete reliably
        }

        public static async Task CreateContainerIfNotExists(CosmosSettings cosmosSettings)
        {
            CreateClient(cosmosSettings, out var cosmosClient);

            await CreateContainerIfNotExists(cosmosSettings, cosmosClient.GetDatabase(cosmosSettings.DatabaseName)).ConfigureAwait(false);
        }

        private static async Task CreateContainerIfNotExists(CosmosSettings cosmosStoreSettings, Database db)
        {
            var containerProperties = new ContainerProperties
            {
                Id = cosmosStoreSettings.ContainerName, PartitionKeyDefinitionVersion = PartitionKeyDefinitionVersion.V2
            };

            if (cosmosStoreSettings.UseHierarchicalPartitionKeys)
            {
                containerProperties.PartitionKeyPaths = new List<string>
                {
                    $"/{nameof(Aggregate.PartitionKeys)}/{nameof(Aggregate.PartitionKeys.Key1)}",
                    $"/{nameof(Aggregate.PartitionKeys)}/{nameof(Aggregate.PartitionKeys.Key2)}",
                    $"/{nameof(Aggregate.PartitionKeys)}/{nameof(Aggregate.PartitionKeys.Key3)}"
                };
            }
            else
            {
                containerProperties.PartitionKeyPath = "/" + nameof(Aggregate.PartitionKey);
            }

            await db.CreateContainerIfNotExistsAsync(containerProperties).ConfigureAwait(false);
        }

        private static async Task<ArrayList> ListContainers(Database database)
        {
            var containers = new ArrayList();

            var iterator = database.GetContainerQueryIterator<ContainerProperties>();
            do
            {
                foreach (var container in await iterator.ReadNextAsync().ConfigureAwait(false))
                    containers.Add(container.Id);
            }
            while (iterator.HasMoreResults);

            return containers;
        }

        private static async Task<ArrayList> ListDatabases(CosmosClient client)
        {
            var databases = new ArrayList();

            var iterator = client.GetDatabaseQueryIterator<DatabaseProperties>();
            do
            {
                foreach (var db in await iterator.ReadNextAsync().ConfigureAwait(false))
                    databases.Add(db.Id);
            }
            while (iterator.HasMoreResults);

            return databases;
        }
    }
}