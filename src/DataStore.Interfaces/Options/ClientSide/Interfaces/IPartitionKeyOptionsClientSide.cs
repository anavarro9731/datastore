namespace DataStore.Interfaces.Options.ClientSide.Interfaces
{
    using System;
    using DataStore.Interfaces.LowLevel;

    
    public interface IPartitionKeyOptionsClientSide<T> where T : IPartitionKeyOptionsClientSide<T>
    {
        T ProvidePartitionKeyValues(Guid tenantId);

        T ProvidePartitionKeyValues(IPartitionKeyTimeInterval timeInterval);

        T ProvidePartitionKeyValues(Guid tenantId, IPartitionKeyTimeInterval timeInterval);

        T AcceptCrossPartitionQueryCost();
    }
}