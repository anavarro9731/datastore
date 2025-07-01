namespace DataStore.Providers.CosmosDb
{
    #region

    using DataStore.Interfaces;
    using Microsoft.Azure.Cosmos;

    #endregion

    public class CosmosSettings : IDatabaseSettings
    {
        public CosmosSettings(string authKey, string containerName, string databaseName, string endpointUrl, CosmosClientOptions clientOptions = null,  bool useHierarchicalPartitionKeys = false)
        {
            AuthKey = authKey;
            DatabaseName = databaseName;
            EndpointUrl = endpointUrl;
            UseHierarchicalPartitionKeys = useHierarchicalPartitionKeys;
            ContainerName = containerName;
            ClientOptions = clientOptions;
            
        }

        public CosmosClientOptions ClientOptions { get; internal set; } 

        public string AuthKey { get; }

        public string DatabaseName { get; }
        
        public string ContainerName { get; }

        public string EndpointUrl { get; }

        public bool UseHierarchicalPartitionKeys { get; }

        public int MaxItemsPerBatchClientDefault => 100;

        public int MaxItemsPerBatchServerLimit => 1000;

        public int MaxQueryCostInRus { get; set; } = 400;

        public IDocumentRepository CreateRepository() => new CosmosDbRepository(this);
    }
}
