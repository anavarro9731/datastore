namespace DataStore.Interfaces.Options
{
    using System;

    public interface IPartitionKeyOptionsClientSide
    {
        void ProvidePartitionKeyValues(Guid tenantId);

        void ProvidePartitionKeyValues(PartitionKeyTimeInterval timeInterval);

        void ProvidePartitionKeyValues(Guid tenantId, PartitionKeyTimeInterval timeInterval);

        void AcceptCrossPartitionQueryCost();
    }
}