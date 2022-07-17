namespace DataStore.Interfaces.Options.LibrarySide
{
    using DataStore.Interfaces.LowLevel.Permissions;
    using DataStore.Interfaces.Options.LibrarySide.Interfaces;

    public class DeleteOptionsLibrarySide : ISecurityOptionsLibrarySide, IOptionsLibrarySide, IPartitionKeyOptionsLibrarySide
    {
        public bool BypassSecurity { get; set; }

        public IIdentityWithDatabasePermissions Identity { get; set; }

        public bool IsHardDelete { get; set; }

        public string PartitionKeyTenantId { get; set; }

        public string PartitionKeyTimeInterval { get; set; }

        public bool AcceptCrossPartitionQueryCost { get; set; }
    }
}