namespace DataStore.Interfaces.Options.LibrarySide.Interfaces
{
    public interface IPartitionKeyOptionsLibrarySide
    {
        string PartitionKeyTenantId { get; set; }
        string PartitionKeyTimeInterval { get; set; }
        bool AcceptCrossPartitionQueryCost { get; set; }
    }
}