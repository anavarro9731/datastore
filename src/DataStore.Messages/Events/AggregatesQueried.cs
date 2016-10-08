namespace DataStore.Messages.Events
{
    using System;
    using System.Linq;

    public class AggregatesQueried<T> : Event, IDataStoreEvent
    {
        public AggregatesQueried(string methodCalled, IQueryable<T> query)
        {
            MethodCalled = methodCalled;
            TypeName = typeof(T).FullName;
            Query = query;
        }

        public IQueryable<T> Query { get; private set; }
        public string TypeName { get; set; }
        public string MethodCalled { get; private set; }
        public double QueryCost { get; set; }
        public TimeSpan QueryDuration { get; set; }
    }
}