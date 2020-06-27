namespace DataStore.Interfaces.LowLevel
{
    using System.Collections.Generic;
    using DataStore.Interfaces.LowLevel.Permissions;

    public interface IAggregate : IHaveScope, IEntity, IRememberWhenIWasModified, IHaveAnETag
    {
        bool Active { get; set; }

        bool ReadOnly { get; set; }

        List<Aggregate.AggregateVersionInfo> VersionHistory { get; set; }
    }
}