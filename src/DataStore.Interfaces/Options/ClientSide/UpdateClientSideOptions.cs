namespace DataStore.Interfaces.Options.ClientSide
{
    #region

    using System;
    using DataStore.Interfaces.LowLevel;
    using DataStore.Interfaces.LowLevel.Permissions;
    using DataStore.Interfaces.Options.ClientSide.Interfaces;
    using DataStore.Interfaces.Options.LibrarySide;

    #endregion

    public class UpdateClientSideOptions : IPartitionKeyOptionsClientSide<UpdateClientSideOptions>,
                                           IPerformanceOptionsClientSide<UpdateClientSideOptions>,
                                           ISecurityOptionsClientSide<UpdateClientSideOptions>
    {
        public UpdateClientSideOptions()
        {
            LibrarySide = new UpdateOptionsLibrarySide();
        }

        protected UpdateOptionsLibrarySide LibrarySide { get; }

        public static implicit operator UpdateOptionsLibrarySide(UpdateClientSideOptions options)
        {
            return options.LibrarySide;
        }

        public UpdateClientSideOptions AcceptCrossPartitionQueryCost()
        {
            LibrarySide.AcceptCrossPartitionQueryCost = true;
            return this;
        }

        public UpdateClientSideOptions AuthoriseFor(IIdentityWithDatabasePermissions identity)
        {
            LibrarySide.Identity = identity;
            return this;
        }

        public UpdateClientSideOptions BypassRULimit(string reason)
        {
            LibrarySide.BypassRULimit = true;
            return this;
        }

        public UpdateClientSideOptions BypassSecurity(string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                throw new ArgumentException("You must provide a reason you are bypassing security. Please be clear. This is for other developers to read.");
            }

            //* reason is only for reading the source code
            LibrarySide.BypassSecurity = true;
            return this;
        }

        public UpdateClientSideOptions DisableOptimisticConcurrency()
        {
            LibrarySide.OptimisticConcurrency = false;
            return this;
        }

        public UpdateClientSideOptions OverwriteReadonly()
        {
            LibrarySide.AllowReadonlyOverwriting = true;
            return this;
        }

        public UpdateClientSideOptions ProvidePartitionKeyValues(Guid tenantId)
        {
            LibrarySide.PartitionKeyTenantId = tenantId.ToString();
            return this;
        }

        public UpdateClientSideOptions ProvidePartitionKeyValues(IPartitionKeyTimeInterval timeInterval)
        {
            LibrarySide.PartitionKeyTimeInterval = timeInterval.ToString();
            return this;
        }

        public UpdateClientSideOptions ProvidePartitionKeyValues(Guid tenantId, IPartitionKeyTimeInterval timeInterval)
        {
            LibrarySide.PartitionKeyTenantId = tenantId.ToString();
            LibrarySide.PartitionKeyTimeInterval = timeInterval.ToString();
            return this;
        }
    }
}