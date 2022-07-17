namespace DataStore.Models.Messages
{
    #region

    using System;
    using DataStore.Interfaces.LowLevel;
    using DataStore.Interfaces.Operations;
    using DataStore.Interfaces.Options;
    using DataStore.Interfaces.Options.LibrarySide.Interfaces;

    #endregion

    public class AggregateQueriedByIdOperationOperation<T> : IDataStoreReadByIdOperation where T: IAggregate
    {
        public AggregateQueriedByIdOperationOperation(string methodCalled, Guid id, IOptionsLibrarySide queryOptions, Type type = null)
        {
            
            MethodCalled = methodCalled;
            Id = id;
            QueryOptions = queryOptions;
            TypeName = type?.FullName;
            Created = DateTime.UtcNow;
        }

        public DateTime Created { get; set; }

        public IOptionsLibrarySide QueryOptions { get; set; }

        public Guid Id { get; set; }

        public string MethodCalled { get; set; }

        public double StateOperationCost { get; set; }

        public TimeSpan? StateOperationDuration { get; set; }

        public long StateOperationStartTimestamp { get; set; }

        public long? StateOperationStopTimestamp { get; set; }

        public string TypeName { get; set; }
    }
    
    public class AggregateQueriedByIdOperationOperation: IDataStoreReadByIdOperation
    {
        public AggregateQueriedByIdOperationOperation(string methodCalled, Guid id, IOptionsLibrarySide queryOptions = null, Type type = null)
        {
            
            MethodCalled = methodCalled;
            Id = id;
            QueryOptions = queryOptions;
            TypeName = type?.FullName;
            Created = DateTime.UtcNow;
        }

        public DateTime Created { get; set; }

        public IOptionsLibrarySide QueryOptions { get; set; }

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