using PalmTree.Infrastructure.Models.Messages;

namespace DataStore.Models.Messages.Events
{
    using System;
    using System.Threading.Tasks;
    using Interfaces;
    using Interfaces.Events;

    public class AggregateAdded<T> : Event<T>, IDataStoreWriteEvent<T> where T : IAggregate
    {
        public AggregateAdded(string methodCalled, T model, IDocumentRepository repo)
            : base(model)
        {
            CommitClosure = async () =>
            {
                await repo.AddAsync(this);
                this.Committed = true;
            };
            TypeName = typeof(T).FullName;
            MethodCalled = methodCalled;
            AggregateId = model.id;
        }

        #region IDataStoreWriteEvent<T> Members

        public Func<Task> CommitClosure { get; set; }
        public bool Committed { get; set; }
        public Guid AggregateId { get; }

        public string TypeName { get; set; }
        public string MethodCalled { get; set; }
        public double QueryCost { get; set; }
        public TimeSpan QueryDuration { get; set; }

        #endregion
    }
}