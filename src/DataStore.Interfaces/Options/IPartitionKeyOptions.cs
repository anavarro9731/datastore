namespace DataStore.Interfaces.Options
{
    public interface IPartitionKeyOptions
    {
        string PartitionKeyTenantId { get; set; }
        string PartitionKeyTimeInterval { get; set; }
    }
}