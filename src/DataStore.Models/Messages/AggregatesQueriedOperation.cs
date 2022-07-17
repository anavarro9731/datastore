namespace DataStore.Models.Messages
{
    #region

    using System;
    using System.Linq;
    using DataStore.Interfaces.LowLevel;
    using DataStore.Interfaces.Operations;
    using DataStore.Interfaces.Options;
    using DataStore.Interfaces.Options.LibrarySide.Interfaces;

    #endregion

    public class AggregatesQueriedOperation<T> : IDataStoreReadFromQueryable<T> where T : class, IAggregate, new()
    {
        public AggregatesQueriedOperation(string methodCalled, IQueryable<T> query, IOptionsLibrarySide queryOptions = null)
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

        public IOptionsLibrarySide QueryOptions { get; set; }

        public double StateOperationCost { get; set; }

        public TimeSpan? StateOperationDuration { get; set; }

        public long StateOperationStartTimestamp { get; set; }

        public long? StateOperationStopTimestamp { get; set; }

        public string TypeName { get; set; }
    }
}