namespace DataStore.Interfaces.Options
{
    #region

    using System;
    using DataStore.Interfaces.LowLevel.Permissions;

    #endregion

    public abstract class ClientSideDeleteOptions : IPartitionKeyOptionsClientSide
    {
        protected ClientSideDeleteOptions(DeleteOptionsLibrarySide librarySide)
        {
            LibrarySide = librarySide;
        }

        protected DeleteOptionsLibrarySide LibrarySide { get; }

        public static implicit operator DeleteOptionsLibrarySide(ClientSideDeleteOptions options)
        {
            return options.LibrarySide;
        }

        //* visible members

        public abstract void AuthoriseFor(IIdentityWithDatabasePermissions identity);

        public abstract void BypassSecurity(string reason);

        public abstract void Permanently();

        public abstract void ProvidePartitionKeyValues(Guid tenantId);

        public abstract void ProvidePartitionKeyValues(PartitionKeyTimeInterval timeInterval);

        public abstract void ProvidePartitionKeyValues(Guid tenantId, PartitionKeyTimeInterval timeInterval);

        public abstract void AcceptCrossPartitionQueryCost();

    }

    public class DeleteOptionsLibrarySide : ISecurityOptions, IQueryOptions, IPartitionKeyOptionsLibrarySide
    {
        public bool BypassSecurity { get; set; }

        public IIdentityWithDatabasePermissions Identity { get; set; }

        public bool IsHardDelete { get; set; }

        public string PartitionKeyTenantId { get; set; }

        public string PartitionKeyTimeInterval { get; set; }

        public bool AcceptCrossPartitionQueryCost { get; set; }
    }
}