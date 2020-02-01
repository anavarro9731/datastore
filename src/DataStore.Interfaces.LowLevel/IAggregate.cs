namespace DataStore.Interfaces.LowLevel
{
    using System;
    using System.Collections.Generic;
    using CircuitBoard.Permissions;

    public interface IAggregate : IHaveScope, IEntity, IRememberWhenIWasModified, IHaveAnETag
    {
        List<Aggregate.AggregateVersionInfo> VersionHistory { get; set; }

        bool Active { get; set; }

        bool ReadOnly { get; set; }
    }
}