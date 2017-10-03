namespace DataStore.Models.Messages
{
    using System;
    using DataStore.Interfaces;

    public class AggregateQueriedByIdOperation : IDataStoreReadById
    {
        public AggregateQueriedByIdOperation(string methodCalled, Guid id, Type type = null)
        {
            MethodCalled = methodCalled;
            Id = id;
            TypeName = type?.FullName;
            Created = DateTime.UtcNow;
        }

        public DateTime Created { get; set; }

        public Guid Id { get; set; }

        public string MethodCalled { get; set; }

        public double StateOperationCost { get; set; }

        public TimeSpan? StateOperationDuration { get; set; }

        public long StateOperationStartTimestamp { get; set; }

        public long? StateOperationStopTimestamp { get; set; }

        public string TypeName { get; set; }
    }
}