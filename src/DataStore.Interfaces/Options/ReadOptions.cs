namespace DataStore.Interfaces.Options
{
    using System;
    using DataStore.Interfaces.LowLevel.Permissions;

    public abstract class ReadOptionsClientSide
    {
        public static implicit operator ReadOptionsLibrarySide(ReadOptionsClientSide options) => options.LibrarySide;

        protected ReadOptionsClientSide(ReadOptionsLibrarySide librarySide)
        {
            LibrarySide = librarySide;
        }

        protected ReadOptionsLibrarySide LibrarySide { get; }

        //* visible members

        public abstract void AuthoriseFor(IIdentityWithDatabasePermissions identity);

        public abstract void BypassSecurity(string reason);

        public abstract void ProvidePartitionKeyValues(Guid tenantId);
        public abstract void ProvidePartitionKeyValues(PartitionKeyTimeInterval timeInterval);
        public abstract void ProvidePartitionKeyValues(Guid tenantId, PartitionKeyTimeInterval timeInterval);
        
    }

    public class ReadOptionsLibrarySide : ISecurityOptions, IPartitionKeyOptions, IQueryOptions
    {
        public IIdentityWithDatabasePermissions Identity { get; set; }

        public bool BypassSecurity { get; set; }

        public string PartitionKeyTenantId { get; set; }

        public string PartitionKeyTimeInterval { get; set; }
    }
}