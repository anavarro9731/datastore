namespace DataStore.Interfaces.Options.ClientSide
{
    using System;
    using DataStore.Interfaces.LowLevel;
    using DataStore.Interfaces.LowLevel.Permissions;
    using DataStore.Interfaces.Options.ClientSide.Interfaces;
    using DataStore.Interfaces.Options.LibrarySide;

    public class DeleteClientSideOptions : IPartitionKeyOptionsClientSide<DeleteClientSideOptions>,
                                           IPerformanceOptionsClientSide<DeleteClientSideOptions>,
                                           ISecurityOptionsClientSide<DeleteClientSideOptions>
    {
        public DeleteClientSideOptions()
        {
            LibrarySide = new DeleteOptionsLibrarySide();
        }

        protected DeleteOptionsLibrarySide LibrarySide { get; }

        public static implicit operator DeleteOptionsLibrarySide(DeleteClientSideOptions options)
        {
            return options.LibrarySide;
        }

        public DeleteClientSideOptions AcceptCrossPartitionQueryCost()
        {
            LibrarySide.AcceptCrossPartitionQueryCost = true;
            return this;
        }

        public DeleteClientSideOptions AuthoriseFor(IIdentityWithDatabasePermissions identity)
        {
            LibrarySide.Identity = identity;
            return this;
        }

        public DeleteClientSideOptions BypassRULimit(string reason)
        {
            LibrarySide.BypassRULimit = true;
            return this;
        }

        public DeleteClientSideOptions BypassSecurity(string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                throw new ArgumentException("You must provide a reason you are bypassing security. Please be clear. This is for other developers to read.");
            }

            //* reason is only for reading the source code
            LibrarySide.BypassSecurity = true;
            return this;
        }

        public DeleteClientSideOptions Permanently()
        {
            LibrarySide.IsHardDelete = true;
            return this;
        }

        public DeleteClientSideOptions ProvidePartitionKeyValues(Guid tenantId)
        {
            LibrarySide.PartitionKeyTenantId = tenantId.ToString();
            return this;
        }

        public DeleteClientSideOptions ProvidePartitionKeyValues(IPartitionKeyTimeInterval timeInterval)
        {
            LibrarySide.PartitionKeyTimeInterval = timeInterval.ToString();
            return this;
        }

        public DeleteClientSideOptions ProvidePartitionKeyValues(Guid tenantId, IPartitionKeyTimeInterval timeInterval)
        {
            LibrarySide.PartitionKeyTenantId = tenantId.ToString();
            LibrarySide.PartitionKeyTimeInterval = timeInterval.ToString();
            return this;
        }
    }
}