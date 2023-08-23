namespace DataStore.Interfaces.Options.ClientSide
{
    using System;
    using DataStore.Interfaces.LowLevel;
    using DataStore.Interfaces.LowLevel.Permissions;
    using DataStore.Interfaces.Options.ClientSide.Interfaces;
    using DataStore.Interfaces.Options.LibrarySide;

    public class WithoutReplayClientSideOptions<T> : IPartitionKeyOptionsClientSide<WithoutReplayClientSideOptions<T>>,
                                                     ISecurityOptionsClientSide<WithoutReplayClientSideOptions<T>>,
                                                     IPerformanceOptionsClientSide<WithoutReplayClientSideOptions<T>> where T : class, IAggregate, new()
    {
        public WithoutReplayClientSideOptions()
        {
            LibrarySide = new WithoutReplayOptionsLibrarySide<T>();
        }

        protected WithoutReplayOptionsLibrarySide<T> LibrarySide { get; }

        public static implicit operator WithoutReplayOptionsLibrarySide<T>(WithoutReplayClientSideOptions<T> options)
        {
            return options.LibrarySide;
        }

        public WithoutReplayClientSideOptions<T> AcceptCrossPartitionQueryCost()
        {
            LibrarySide.AcceptCrossPartitionQueryCost = true;
            return this;
        }

        public WithoutReplayClientSideOptions<T> AuthoriseFor(IIdentityWithDatabasePermissions identity)
        {
            LibrarySide.Identity = identity;
            return this;
        }

        public WithoutReplayClientSideOptions<T> BypassRULimit(string reason)
        {
            LibrarySide.BypassRULimit = true;
            return this;
        }

        public WithoutReplayClientSideOptions<T> BypassSecurity(string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                throw new ArgumentException("You must provide a reason you are bypassing security. Please be clear. This is for other developers to read.");
            }

            //* reason is only for reading the source code
            LibrarySide.BypassSecurity = true;
            return this;
        }

        public WithoutReplayClientSideOptions<T> ProvidePartitionKeyValues(Guid tenantId)
        {
            LibrarySide.PartitionKeyTenantId = tenantId.ToString();
            return this;
        }

        public WithoutReplayClientSideOptions<T> ProvidePartitionKeyValues(IPartitionKeyTimeInterval timeInterval)
        {
            LibrarySide.PartitionKeyTimeInterval = timeInterval.ToString();
            return this;
        }

        public WithoutReplayClientSideOptions<T> ProvidePartitionKeyValues(Guid tenantId, IPartitionKeyTimeInterval timeInterval)
        {
            LibrarySide.PartitionKeyTenantId = tenantId.ToString();
            LibrarySide.PartitionKeyTimeInterval = timeInterval.ToString();
            return this;
        }
    }
}