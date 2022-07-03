namespace DataStore.Models.Messages
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DataStore.Interfaces;
    using DataStore.Interfaces.LowLevel;
    using DataStore.Interfaces.Operations;

    public class AggregateQueriedByIdOperationOperation<T> : IDataStoreReadByIdOperation where T: IAggregate
    {
        public AggregateQueriedByIdOperationOperation(string methodCalled, Guid id, PartitionKeySettings partitionKeySettings, Type type = null)
        {
            
            MethodCalled = methodCalled;
            Id = id;
            PartitionKey = partitionKeySettings.GetKey<T>(id);
            TypeName = type?.FullName;
            Created = DateTime.UtcNow;
        }

        public DateTime Created { get; set; }

        public Guid Id { get; set; }

        public string PartitionKey { get; set; }

        public string MethodCalled { get; set; }

        public double StateOperationCost { get; set; }

        public TimeSpan? StateOperationDuration { get; set; }

        public long StateOperationStartTimestamp { get; set; }

        public long? StateOperationStopTimestamp { get; set; }

        public string TypeName { get; set; }
    }
    
    public class AggregateQueriedByIdOperationOperation: IDataStoreReadByIdOperation
    {
        public AggregateQueriedByIdOperationOperation(string methodCalled, Guid id, string partitionKey, Type type = null)
        {
            
            MethodCalled = methodCalled;
            Id = id;
            PartitionKey = partitionKey;
            TypeName = type?.FullName;
            Created = DateTime.UtcNow;
        }

        public DateTime Created { get; set; }

        public Guid Id { get; set; }

        public string PartitionKey { get; set; }

        public string MethodCalled { get; set; }

        public double StateOperationCost { get; set; }

        public TimeSpan? StateOperationDuration { get; set; }

        public long StateOperationStartTimestamp { get; set; }

        public long? StateOperationStopTimestamp { get; set; }

        public string TypeName { get; set; }
    }
}