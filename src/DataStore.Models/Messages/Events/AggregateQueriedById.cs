namespace DataStore.Models.Messages.Events
{
    using System;
    using Interfaces.Events;

    public class AggregateQueriedById : Event, IDataStoreReadById
    {
        public AggregateQueriedById(string methodCalled, Guid id, Type type = null)
        {
            MethodCalled = methodCalled;
            Id = id;
            TypeName = type?.FullName;
        }

        public string TypeName { get; set; }
        public string MethodCalled { get; set; }
        public Guid Id { get; set; }
        public double QueryCost { get; set; }
        public TimeSpan QueryDuration { get; set; }
    }
}