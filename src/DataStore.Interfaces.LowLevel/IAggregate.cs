namespace DataStore.Interfaces.LowLevel
{
    #region

    using System.Collections.Generic;
    using DataStore.Interfaces.LowLevel.Permissions;

    #endregion

    public interface IAggregate : IHaveScope, IEntity, IRememberWhenIWasModified, IHaveAnETag
    {
        bool Active { get; set; }

        string PartitionKey { get; set; }

        HierarchicalPartitionKey PartitionKeys { get; set; }

        bool ReadOnly { get; set; }

        List<Aggregate.AggregateVersionInfo> VersionHistory { get; set; }


    }
}