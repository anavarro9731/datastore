namespace DataStore.Providers.CosmosDb
{
    using DataStore.Interfaces.LowLevel;
    using Microsoft.Azure.Cosmos;

    public static class Ext
    {
        public static PartitionKey ToCosmosPartitionKey(this Aggregate.HierarchicalPartitionKey hierarchicalPartitionKey)
        {
            var builder = new PartitionKeyBuilder();
            if (!string.IsNullOrWhiteSpace(hierarchicalPartitionKey.Key1)) builder.Add(hierarchicalPartitionKey.Key1);
            if (!string.IsNullOrWhiteSpace(hierarchicalPartitionKey.Key2)) builder.Add(hierarchicalPartitionKey.Key2);
            if (!string.IsNullOrWhiteSpace(hierarchicalPartitionKey.Key3)) builder.Add(hierarchicalPartitionKey.Key3);
            var key = builder.Build();
            return key;
        }
    }
}