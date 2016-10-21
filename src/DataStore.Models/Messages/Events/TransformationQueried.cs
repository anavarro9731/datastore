using System;
using System.Linq;
using DataStore.DataAccess.Interfaces.Events;

namespace DataStore.DataAccess.Models.Messages.Events
{
    public class TransformationQueried<T> : Event, IDataStoreReadFromQueryable<T>
    {
        public TransformationQueried(string methodCalled, IQueryable<T> query)
        {
            MethodCalled = methodCalled;
            TypeName = typeof(T).FullName;
            Query = query;
        }

        public IQueryable<T> Query { get; set; }
        public string TypeName { get; set; }
        public string MethodCalled { get; set; }
        public double QueryCost { get; set; }
        public TimeSpan QueryDuration { get; set; }
    }
}