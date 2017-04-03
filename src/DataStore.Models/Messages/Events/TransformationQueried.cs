namespace DataStore.Models.Messages.Events
{
    using System;
    using System.Linq;
    using Interfaces.Events;

    public class TransformationQueried<T> : IDataStoreReadFromQueryable<T>
    {
        public TransformationQueried(string methodCalled, IQueryable<T> query)
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
        public TimeSpan StateOperationDuration { get; set; }
        public DateTime Created { get; set; }
    }
}