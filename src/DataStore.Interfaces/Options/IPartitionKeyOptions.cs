namespace DataStore.Interfaces.Options
{
    using System;

    public interface IPartitionKeyOptions
    {
        string PartitionKeyTenantId { get; set; }
        string PartitionKeyTimeInterval { get; set; }
    }
}