namespace DataStore.Models.Messages.Events
{
    using System;
    using System.Threading.Tasks;
    using Interfaces;
    using Interfaces.Events;

    public class AggregateUpdated<T> : Event<T>, IDataStoreWriteEvent<T> where T : IAggregate
    {
        public AggregateUpdated(string methodCalled, T model, IDocumentRepository repository)
            : base(model)
        {
            CommitClosure = async () =>
            {
                await repository.UpdateAsync(this);
                this.Committed = true;
            };

            this.MethodCalled = methodCalled;
            this.TypeName = typeof(T).FullName;
            AggregateId = model.id;
        }

        #region IDataStoreWriteEvent<T> Members

        public string TypeName { get; set; }
        public string MethodCalled { get; set; }
        public double QueryCost { get; set; }
        public TimeSpan QueryDuration { get; set; }
        public Func<Task> CommitClosure { get; set; }
        public bool Committed { get; set; }
        public Guid AggregateId { get; }

        #endregion
    }
}