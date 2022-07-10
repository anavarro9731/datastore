namespace DataStore.Interfaces.Options
{
    using System;
    using DataStore.Interfaces.LowLevel.Permissions;

    public abstract class ClientSideReadOptions
    {
        protected ClientSideReadOptions(ReadOptionsLibrarySide librarySide)
        {
            LibrarySide = librarySide;
        }

        protected ReadOptionsLibrarySide LibrarySide { get; }

        public static implicit operator ReadOptionsLibrarySide(ClientSideReadOptions options)
        {
            return options.LibrarySide;
        }

        //* visible members

        public abstract void AuthoriseFor(IIdentityWithDatabasePermissions identity);

        public abstract void BypassSecurity(string reason);

        public abstract void ProvidePartitionKeyValues(Guid tenantId);

        public abstract void ProvidePartitionKeyValues(PartitionKeyTimeInterval timeInterval);

        public abstract void ProvidePartitionKeyValues(Guid tenantId, PartitionKeyTimeInterval timeInterval);
    }

    public class ReadOptionsLibrarySide : ISecurityOptions, IPartitionKeyOptions, IQueryOptions
    {
        public bool BypassSecurity { get; set; }

        public IIdentityWithDatabasePermissions Identity { get; set; }

        public string PartitionKeyTenantId { get; set; }

        public string PartitionKeyTimeInterval { get; set; }
    }
}