namespace DataStore.Options
{
    using System;
    using global::DataStore.Interfaces;
    using global::DataStore.Interfaces.LowLevel.Permissions;
    using global::DataStore.Interfaces.Options;

    public class DefaultUpdateOptions : UpdateOptionsClientSide
    {
        public DefaultUpdateOptions()
            : base(new UpdateOptionsLibrarySide())
        {
            /* use constructors on derived classes to input a more advanced library side
             which we could then cast to in the additional interface methods below to 
            set its advanced properties */
        }

        public override void AuthoriseFor(IIdentityWithDatabasePermissions identity)
        {
            LibrarySide.Identity = identity;
        }

        public override void DisableOptimisticConcurrency()
        {
            LibrarySide.OptimisticConcurrency = false;
        }

        public override void OverwriteReadonly()
        {
            LibrarySide.AllowReadonlyOverwriting = true;
        }
        
                
        public override void BypassSecurity(string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("You must provide a reason you are bypassing security. Please be clear. This is for other developers to read.");
            //* reason is only for reading the source code
            LibrarySide.BypassSecurity = true;
        }
        
        public override void ProvidePartitionKeyValues(Guid tenantId)
        {
            LibrarySide.PartitionKeyTenantId = tenantId;
        }

        public override void ProvidePartitionKeyValues(PartitionKeyTimeInterval timeInterval)
        {
            LibrarySide.PartitionKeyTimeInterval = timeInterval;
        }

        public override void ProvidePartitionKeyValues(Guid tenantId, PartitionKeyTimeInterval timeInterval)
        {
            LibrarySide.PartitionKeyTenantId = tenantId;
            LibrarySide.PartitionKeyTimeInterval = timeInterval;
        }
    }
}