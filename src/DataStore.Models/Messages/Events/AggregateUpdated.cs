namespace DataStore.Models.Messages.Events
{
    using System;
    using System.Threading.Tasks;
    using Interfaces;
    using Interfaces.Events;

    public class AggregateUpdated<T> : IDataStoreWriteEvent<T> where T : IAggregate
    {
        public AggregateUpdated(string methodCalled, T model, IDocumentRepository repository)
        {
            CommitClosure = async () =>
            {
                await repository.UpdateAsync(this);
                Committed = true;
            };

            MethodCalled = methodCalled;
            TypeName = typeof(T).FullName;
            AggregateId = model.Id;
            Model = model;
            Created = DateTime.UtcNow;
            MessageId = Guid.NewGuid();
        }

        #region IDataStoreWriteEvent<T> Members

        public string TypeName { get; set; }
        public string MethodCalled { get; set; }
        public double QueryCost { get; set; }
        public TimeSpan QueryDuration { get; set; }
        public Func<Task> CommitClosure { get; set; }
        public bool Committed { get; set; }
        public Guid AggregateId { get; }
        public T Model { get; }
        public DateTime Created { get; }
        public Guid MessageId { get; }

        #endregion
    }
}