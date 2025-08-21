namespace DataStore.Providers.CosmosDb
{
    #region

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Threading.Tasks;
    using CircuitBoard;
    using DataStore.Interfaces;
    using DataStore.Interfaces.LowLevel;
    using Microsoft.Azure.Cosmos;

    #endregion

    public class CosmosDbUtilities : IDatabaseUtilities
    {
        public static async Task CreateContainerIfNotExists(CosmosSettings cosmosSettings, bool requireSharedThroughput = false)
        {
            CreateClient(cosmosSettings, out var cosmosClient);

            await CreateContainerIfNotExists(cosmosSettings, cosmosClient.GetDatabase(cosmosSettings.DatabaseName), requireSharedThroughput).ConfigureAwait(false);
        }

        public async Task CreateDatabaseIfNotExists(IDatabaseSettings cosmosStoreSettings, bool useSharedThroughput)
        {
            {
                CreateClient((CosmosSettings)cosmosStoreSettings, out var cosmosClient);
                await CreateDbAndContainerIfNotExists(cosmosClient, (CosmosSettings)cosmosStoreSettings, useSharedThroughput).ConfigureAwait(false);
            }
        }

        public async Task ResetDatabase(IDatabaseSettings cosmosStoreSettings, bool useSharedThroughput = false)
        {
            {
                var cosmosSettings = (CosmosSettings)cosmosStoreSettings;

                CreateClient(cosmosSettings, out var cosmosClient);
                await DeleteContainerIfDbExists(cosmosClient, cosmosSettings).ConfigureAwait(false);
                await CreateDbAndContainerIfNotExists(cosmosClient, cosmosSettings, useSharedThroughput).ConfigureAwait(false);
            }

            async Task DeleteContainerIfDbExists(CosmosClient client, CosmosSettings cosmosSettings)
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

        private static async Task CreateContainerIfNotExists(CosmosSettings cosmosStoreSettings, Database db, bool requireSharedThroughput)
        {
            // Check if database has shared throughput configured
            var databaseThroughput = await db.ReadThroughputAsync().ConfigureAwait(false);
            
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
            
            if (requireSharedThroughput)
            {
                if (databaseThroughput == null)
                {
                    throw new InvalidOperationException(
                        $"Database '{cosmosStoreSettings.DatabaseName}' does not have shared throughput configured. "
                        + "Cannot create container without dedicated RUs. Please recreate the database with shared throughput.");
                }
                // Create container so it requires shared database throughput
                await db.CreateContainerIfNotExistsAsync(containerProperties, throughput: null).ConfigureAwait(false);
            }
            else
            {
                // Create container without specifying throughput so it uses the database settings to determine the container throughput
                await db.CreateContainerIfNotExistsAsync(containerProperties).ConfigureAwait(false);
            }
        }

        private static async Task CreateDbAndContainerIfNotExists(CosmosClient client, CosmosSettings cosmosStoreSettings, bool useSharedThroughput)
        {
            var databases = await ListDatabases(client).ConfigureAwait(false);

            Database db;

            if (!databases.Contains(cosmosStoreSettings.DatabaseName))
            {
                
                // Create database with shared throughput (400 RU minimum) so containers can share RUs
                db = await client.CreateDatabaseAsync(
                         cosmosStoreSettings.DatabaseName,  
                         throughput: useSharedThroughput? 400: (int?)null).ConfigureAwait(false);
            }
            else
            {
                db = client.GetDatabase(cosmosStoreSettings.DatabaseName);
            }

            await CreateContainerIfNotExists(cosmosStoreSettings, db, useSharedThroughput).ConfigureAwait(false);

            
            /* in some tests the above call seemed not to wait for the container to be ready before returning
             not completely sure if this is still the case, but will keep the check below */
            
            int tries = 0;
            while (++tries <= 10)
            {
                await Task.Delay(1000).ConfigureAwait(false);
                using (FeedIterator<ContainerProperties> resultSetIterator = db.GetContainerQueryIterator<ContainerProperties>())
                {
                    while (resultSetIterator.HasMoreResults)
                    {
                        foreach (ContainerProperties container in await resultSetIterator.ReadNextAsync().ConfigureAwait(false))
                        {
                            if (container.Id == cosmosStoreSettings.ContainerName)
                            {
                                goto containerFound;
                            }
                        }
                    }
                }
                
            } 
            containerFound:

            if (tries == 10) throw new CircuitException("Container was not created after 10 seconds");
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