namespace DataStore.Interfaces.Options.ClientSide
{
    #region

    using System;
    using DataStore.Interfaces.LowLevel;
    using DataStore.Interfaces.Options.ClientSide.Interfaces;

    #endregion

    public class UpdateClientSideOptions : UpdateClientSideBaseOptions, IPartitionKeyOptionsClientSide, IPerformanceOptionsClientSide
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

        internal void ProvidePartitionKeyValues(string tenantId, string timeInterval)
        {
            LibrarySide.PartitionKeyTenantId = tenantId;
            LibrarySide.PartitionKeyTimeInterval = timeInterval;
        }

        public void BypassRULimit(string reason)
        {
            LibrarySide.BypassRULimit = true;
        }
    }
}