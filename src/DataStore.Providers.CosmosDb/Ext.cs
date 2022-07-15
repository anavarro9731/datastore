namespace DataStore.Providers.CosmosDb
{
    #region

    using DataStore.Interfaces.LowLevel;
    using Microsoft.Azure.Cosmos;

    #endregion

    public static class Ext
    {
        public static PartitionKey ToCosmosPartitionKey(this HierarchicalPartitionKey hierarchicalPartitionKey, bool useHierarchicalPartitionKeys)
        {
            if (!useHierarchicalPartitionKeys) return new PartitionKey(hierarchicalPartitionKey.ToSyntheticKeyString());
            
            var builder = new PartitionKeyBuilder();
            builder.Add(hierarchicalPartitionKey.Key1);
            builder.Add(hierarchicalPartitionKey.Key2);
            builder.Add(hierarchicalPartitionKey.Key3);
            var key = builder.Build();
            return key;

        }
    }
}