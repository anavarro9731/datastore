namespace DataStore.Interfaces.Options
{
    using System;
    using DataStore.Interfaces.LowLevel.Permissions;

    public abstract class ClientSideUpdateOptions
    {
        protected ClientSideUpdateOptions(UpdateOptionsLibrarySide librarySide)
        {
            LibrarySide = librarySide;
        }

        protected UpdateOptionsLibrarySide LibrarySide { get; }

        public static implicit operator UpdateOptionsLibrarySide(ClientSideUpdateOptions options)
        {
            return options.LibrarySide;
        }

        public abstract void AuthoriseFor(IIdentityWithDatabasePermissions identity);

        public abstract void BypassSecurity(string reason);

        public abstract void DisableOptimisticConcurrency();

        public abstract void OverwriteReadonly();

        public abstract void ProvidePartitionKeyValues(Guid tenantId);

        public abstract void ProvidePartitionKeyValues(PartitionKeyTimeInterval timeInterval);

        public abstract void ProvidePartitionKeyValues(Guid tenantId, PartitionKeyTimeInterval timeInterval);
    }

    public class UpdateOptionsLibrarySide : ISecurityOptions, IQueryOptions, IPartitionKeyOptions
    {
        public bool AllowReadonlyOverwriting { get; set; }

        public bool BypassSecurity { get; set; }

        public IIdentityWithDatabasePermissions Identity { get; set; }

        public bool OptimisticConcurrency { get; set; } = true;

        public string PartitionKeyTenantId { get; set; }

        public string PartitionKeyTimeInterval { get; set; }
    }
}