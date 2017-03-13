namespace DataStore.Models.Messages.Events
{
    using System;
    using System.Linq;
    using Interfaces.Events;

    public class AggregatesQueried<T> : IDataStoreReadFromQueryable<T>
    {
        public AggregatesQueried(string methodCalled, IQueryable<T> query)
        {
            MethodCalled = methodCalled;
            TypeName = typeof(T).FullName;
            Query = query;
            Created = DateTime.UtcNow;
            MessageId = Guid.NewGuid();
        }

        public IQueryable<T> Query { get; set; }
        public string TypeName { get; set; }
        public string MethodCalled { get; set; }
        public double QueryCost { get; set; }
        public TimeSpan QueryDuration { get; set; }
        public DateTime Created { get; }
        public Guid MessageId { get; }
    }
}