namespace DataStore.Interfaces.Options
{
    using System;

    public interface IPartitionKeyOptions
    {
        Guid? PartitionKeyTenantId { get; set; }
        PartitionKeyTimeInterval PartitionKeyTimeInterval { get; set; }
    }
}