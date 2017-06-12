using System;
using DataStore.Interfaces;

namespace DataStore.Models.Messages
{
    public class AggregateQueriedByIdOperation : IDataStoreReadById
    {
        public AggregateQueriedByIdOperation(string methodCalled, Guid id, Type type = null)
        {
            MethodCalled = methodCalled;
            Id = id;
            TypeName = type?.FullName;
            Created = DateTime.UtcNow;
        }

        public string TypeName { get; set; }
        public string MethodCalled { get; set; }
        public Guid Id { get; set; }
        public double StateOperationCost { get; set; }
        public TimeSpan StateOperationDuration { get; set; }
        public DateTime Created { get; set; }
    }
}