namespace DataStore.Interfaces.Options.ClientSide
{
    using System;
    using DataStore.Interfaces.LowLevel;
    using DataStore.Interfaces.LowLevel.Permissions;
    using DataStore.Interfaces.Options.ClientSide.Interfaces;
    using DataStore.Interfaces.Options.LibrarySide;

    public class ReadClientSideOptions : IPartitionKeyOptionsClientSide<ReadClientSideOptions>,
                                         IPerformanceOptionsClientSide<ReadClientSideOptions>,
                                         ISecurityOptionsClientSide<ReadClientSideOptions>
    {
        public ReadClientSideOptions()
        {
            LibrarySide = new ReadOptionsLibrarySide();
        }

        protected ReadOptionsLibrarySide LibrarySide { get; }

        public static implicit operator ReadOptionsLibrarySide(ReadClientSideOptions options)
        {
            return options.LibrarySide;
        }

        public ReadClientSideOptions AcceptCrossPartitionQueryCost()
        {
            LibrarySide.AcceptCrossPartitionQueryCost = true;
            return this;
        }

        public ReadClientSideOptions AuthoriseFor(IIdentityWithDatabasePermissions identity)
        {
            LibrarySide.Identity = identity;
            return this;
        }

        public ReadClientSideOptions BypassRULimit(string reason)
        {
            LibrarySide.BypassRULimit = true;
            return this;
        }

        public ReadClientSideOptions BypassSecurity(string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                throw new ArgumentException("You must provide a reason you are bypassing security. Please be clear. This is for other developers to read.");
            }

            //* reason is only for reading the source code
            LibrarySide.BypassSecurity = true;
            return this;
        }

        public ReadClientSideOptions ProvidePartitionKeyValues(Guid tenantId)
        {
            LibrarySide.PartitionKeyTenantId = tenantId.ToString();
            return this;
        }

        public ReadClientSideOptions ProvidePartitionKeyValues(IPartitionKeyTimeInterval timeInterval)
        {
            LibrarySide.PartitionKeyTimeInterval = timeInterval.ToString();
            return this;
        }

        public ReadClientSideOptions ProvidePartitionKeyValues(Guid tenantId, IPartitionKeyTimeInterval timeInterval)
        {
            LibrarySide.PartitionKeyTenantId = tenantId.ToString();
            LibrarySide.PartitionKeyTimeInterval = timeInterval.ToString();
            return this;
        }
    }
}