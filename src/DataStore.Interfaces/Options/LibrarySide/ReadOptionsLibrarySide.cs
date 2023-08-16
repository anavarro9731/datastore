namespace DataStore.Interfaces.Options.LibrarySide
{
    using DataStore.Interfaces.LowLevel.Permissions;
    using DataStore.Interfaces.Options.LibrarySide.Interfaces;

    public class ReadOptionsLibrarySide : ISecurityOptionsLibrarySide, IPartitionKeyOptionsLibrarySide, IOptionsLibrarySide, IPerformanceOptionsLibrarySide
    {
        public bool BypassRULimit { get; set; }
        
        public bool BypassSecurity { get; set; }

        public IIdentityWithDatabasePermissions Identity { get; set; }

        public string PartitionKeyTenantId { get; set; }

        public string PartitionKeyTimeInterval { get; set; }

        public bool AcceptCrossPartitionQueryCost { get; set; }
    }
}