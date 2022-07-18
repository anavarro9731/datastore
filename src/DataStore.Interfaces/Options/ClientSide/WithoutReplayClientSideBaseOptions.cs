namespace DataStore.Interfaces.Options.ClientSide
{
    using System;
    using DataStore.Interfaces.LowLevel;
    using DataStore.Interfaces.LowLevel.Permissions;
    using DataStore.Interfaces.Options.ClientSide.Interfaces;
    using DataStore.Interfaces.Options.LibrarySide;

    public abstract class WithoutReplayClientSideBaseOptions<T> : IPartitionKeyOptionsClientSide, ISecurityOptionsClientSide
    {
        protected WithoutReplayClientSideBaseOptions()
        {
            LibrarySide = new WithoutReplayOptionsLibrarySide<T>();
        }
        
        protected WithoutReplayOptionsLibrarySide<T> LibrarySide { get; }

        public static implicit operator WithoutReplayOptionsLibrarySide<T>(WithoutReplayClientSideBaseOptions<T> options)
        {
            return options.LibrarySide;
        }
        
        public void AuthoriseFor(IIdentityWithDatabasePermissions identity)
        {
            LibrarySide.Identity = identity;
        }
        
        public void AcceptCrossPartitionQueryCost()
        {
            LibrarySide.AcceptCrossPartitionQueryCost = true;
        }
        
        public void BypassSecurity(string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("You must provide a reason you are bypassing security. Please be clear. This is for other developers to read.");
            //* reason is only for reading the source code
            LibrarySide.BypassSecurity = true;
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