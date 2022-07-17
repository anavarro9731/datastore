namespace DataStore.Interfaces.Options.LibrarySide
{
    using DataStore.Interfaces.LowLevel.Permissions;
    using DataStore.Interfaces.Options.LibrarySide.Interfaces;

    public class UpdateOptionsLibrarySide : ISecurityOptionsLibrarySide, IOptionsLibrarySide, IPartitionKeyOptionsLibrarySide
    {
        public bool AllowReadonlyOverwriting { get; set; }

        public bool BypassSecurity { get; set; }

        public IIdentityWithDatabasePermissions Identity { get; set; }

        public bool OptimisticConcurrency { get; set; } = true;

        public string PartitionKeyTenantId { get; set; }

        public string PartitionKeyTimeInterval { get; set; }

        public bool AcceptCrossPartitionQueryCost { get; set; }
    }
}