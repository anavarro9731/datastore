namespace DataStore.Models.Messages
{
    using System;
    using System.Linq;
    using DataStore.Interfaces;
    using DataStore.Interfaces.LowLevel;

    public class AggregatesQueriedOperation<T> : IDataStoreReadFromQueryable<T> where T : class, IAggregate, new()
    {
        public AggregatesQueriedOperation(string methodCalled, IQueryable<T> query, IQueryOptions<T> queryOptions = null)
        {
            MethodCalled = methodCalled;
            TypeName = typeof(T).FullName;
            Query = query;
            QueryOptions = queryOptions;
            Created = DateTime.UtcNow;  
        }

        public DateTime Created { get; set; }

        public string MethodCalled { get; set; }

        public IQueryable<T> Query { get; set; }

        public IQueryOptions<T> QueryOptions { get; set; }

        public double StateOperationCost { get; set; }

        public TimeSpan? StateOperationDuration { get; set; }

        public long StateOperationStartTimestamp { get; set; }

        public long? StateOperationStopTimestamp { get; set; }

        public string TypeName { get; set; }
    }
}