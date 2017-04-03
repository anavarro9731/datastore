namespace DataStore.Models.Messages.Events
{
    using System;
    using System.Threading.Tasks;
    using Interfaces;
    using Interfaces.Events;
    using Interfaces.LowLevel;

    public class AggregateAdded<T> : IDataStoreWriteEvent<T> where T : IAggregate
    {
        public AggregateAdded(string methodCalled, T model, IDocumentRepository repo)
        {
            CommitClosure = async () =>
            {
                await repo.AddAsync(this);
                Committed = true;
            };
            TypeName = typeof(T).FullName;
            MethodCalled = methodCalled;
            AggregateId = model.id;
            Model = model;
            Created = DateTime.UtcNow;
        }

        #region IDataStoreWriteEvent<T> Members

        public Func<Task> CommitClosure { get; set; }
        public bool Committed { get; set; }
        public Guid AggregateId { get; set; }

        public string TypeName { get; set; }
        public string MethodCalled { get; set; }
        public double StateOperationCost { get; set; }
        public TimeSpan StateOperationDuration { get; set; }
        public T Model { get; set; }
        public DateTime Created { get; set; }

        #endregion
    }
}