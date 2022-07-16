namespace DataStore.Interfaces.Options
{
    public interface IPartitionKeyOptionsLibrarySide
    {
        string PartitionKeyTenantId { get; set; }
        string PartitionKeyTimeInterval { get; set; }
        bool AcceptCrossPartitionQueryCost { get; set; }
    }
}