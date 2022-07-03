namespace DataStore.Interfaces.LowLevel
{
    using System;
    using System.Collections.Generic;
    using DataStore.Interfaces.LowLevel.Permissions;

    public interface IAggregate : IHaveScope, IEntity, IRememberWhenIWasModified, IHaveAnETag
    {
        bool Active { get; set; }

        string PartitionKey { get; set; }

        Aggregate.HierarchicalPartitionKey PartitionKeys { get; set; }

        bool ReadOnly { get; set; }

        List<Aggregate.AggregateVersionInfo> VersionHistory { get; set; }


    }
}