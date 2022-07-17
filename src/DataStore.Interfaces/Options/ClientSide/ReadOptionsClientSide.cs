namespace DataStore.Interfaces.Options.ClientSide
{
    using System;
    using DataStore.Interfaces.LowLevel;
    using DataStore.Interfaces.Options.ClientSide.Interfaces;

    public class ReadOptionsClientSide : ReadOptionsClientSideBase, IPartitionKeyOptionsClientSide
    {
        public void AcceptCrossPartitionQueryCost()
        {
            LibrarySide.AcceptCrossPartitionQueryCost = true;
        }

        public void ProvidePartitionKeyValues(Guid tenantId)
        {
            LibrarySide.PartitionKeyTenantId = tenantId.ToString();
        }

        public void ProvidePartitionKeyValues(IPartitionKeyTimeInterval timeInterval)
        {
            LibrarySide.PartitionKeyTimeInterval = timeInterval.ToString();
        }

        public void ProvidePartitionKeyValues(Guid tenantId, IPartitionKeyTimeInterval timeInterval)
        {
            LibrarySide.PartitionKeyTenantId = tenantId.ToString();
            LibrarySide.PartitionKeyTimeInterval = timeInterval.ToString();
        }
    }
}