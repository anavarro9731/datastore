namespace DataStore.Messages.Events
{
    using System;

    public class AggregateQueriedById : Event, IDataStoreEvent
    {
        public AggregateQueriedById(string methodCalled, Guid id, Type type = null)
        {
            MethodCalled = methodCalled;
            Id = id;
            TypeName = type?.FullName;
        }

        public string TypeName { get; private set; }
        public string MethodCalled { get; private set; }
        public Guid Id { get; private set; }
        public double QueryCost { get; set; }
        public TimeSpan QueryDuration { get; set; }
    }
}