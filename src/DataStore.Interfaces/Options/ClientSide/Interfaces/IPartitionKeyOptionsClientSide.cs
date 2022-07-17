namespace DataStore.Interfaces.Options.ClientSide.Interfaces
{
    using System;
    using DataStore.Interfaces.LowLevel;

    public interface IPartitionKeyOptionsClientSide
    {
        void ProvidePartitionKeyValues(Guid tenantId);

        void ProvidePartitionKeyValues(IPartitionKeyTimeInterval timeInterval);

        void ProvidePartitionKeyValues(Guid tenantId, IPartitionKeyTimeInterval timeInterval);

        void AcceptCrossPartitionQueryCost();
    }
}