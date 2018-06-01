namespace DataStore.Interfaces.LowLevel
{
    using System;
    using System.Collections.Generic;

    public interface IAggregateHistory<TAggregate> where TAggregate : IAggregate
    {
        List<IAggregateHistoryItemHeader> AggregateVersions { get; set; }

        Guid AggregateId { get; set; }

        int Version { get; set; }
    }

    public class AggregateHistory<TAggregate> : Aggregate, IAggregateHistory<TAggregate> where TAggregate : IAggregate
    {
        public List<IAggregateHistoryItemHeader> AggregateVersions { get; set; }

        public Guid AggregateId { get; set; }

        public int Version { get; set; }
    }

    public interface IAggregateHistoryItemHeader
    {
        Guid AggegateHistoryItemId { get; set; }

        string AssemblyQualifiedTypeName { get; set; }

        Guid UnitWorkId { get; set; }

        DateTime VersionedAt { get; set; }

        int VersionId { get; set; }
    }

    public class AggregateHistoryItemHeader : IAggregateHistoryItemHeader
    {
        public Guid AggegateHistoryItemId { get; set; }

        public string AssemblyQualifiedTypeName { get; set; }

        public Guid UnitWorkId { get; set; }

        public DateTime VersionedAt { get; set; }

        public int VersionId { get; set; }
    }
}