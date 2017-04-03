namespace DataStore.Models.Messages.Events
{
    using System;
    using System.Threading.Tasks;
    using Interfaces;
    using Interfaces.Events;
    using Interfaces.LowLevel;

    public class AggregateHardDeleted<T> : IDataStoreWriteEvent<T> where T : IAggregate
    {
        public AggregateHardDeleted(string methodCalled, T model, IDocumentRepository repository)
        {
            CommitClosure = async () =>
            {
                await repository.DeleteHardAsync(this);
                Committed = true;
            };
            MethodCalled = methodCalled;
            TypeName = typeof(T).FullName;
            AggregateId = model.id;
            Model = model;
            Created = DateTime.UtcNow;
        }

        #region IDataStoreWriteEvent<T> Members

        public string TypeName { get; set; }
        public string MethodCalled { get; set; }
        public double StateOperationCost { get; set; }
        public TimeSpan StateOperationDuration { get; set; }
        public Func<Task> CommitClosure { get; set; }
        public bool Committed { get; set; }
        public Guid AggregateId { get; set; }
        public T Model { get; set; }
        public DateTime Created { get; set; }

        #endregion
    }
}