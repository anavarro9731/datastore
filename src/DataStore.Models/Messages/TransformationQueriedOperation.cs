using System;
using System.Linq;
using DataStore.Interfaces;

namespace DataStore.Models.Messages
{
    public class TransformationQueriedOperation<T> : IDataStoreReadFromQueryable<T>
    {
        public TransformationQueriedOperation(string methodCalled, IQueryable<T> query)
        {
            MethodCalled = methodCalled;
            TypeName = typeof(T).FullName;
            Query = query;
            Created = DateTime.UtcNow;
        }

        public IQueryable<T> Query { get; set; }
        public string TypeName { get; set; }
        public string MethodCalled { get; set; }
        public double StateOperationCost { get; set; }
        public long StateOperationStartTimestamp { get; set; }
        public long? StateOperationStopTimestamp { get; set; }
        public TimeSpan? StateOperationDuration { get; set; }
        public DateTime Created { get; set; }
    }
}