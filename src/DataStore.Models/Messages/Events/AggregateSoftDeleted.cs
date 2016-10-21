using System;
using System.Threading.Tasks;
using DataStore.DataAccess.Interfaces;
using DataStore.DataAccess.Interfaces.Events;

namespace DataStore.DataAccess.Models.Messages.Events
{
    public class AggregateSoftDeleted<T> : Event<T>, IDataStoreWriteEvent<T> where T : IAggregate
    {
        public AggregateSoftDeleted(string methodCalled, T model, IDocumentRepository repository)
            : base(model)
        {
            CommitClosure = async () =>
            {
                await repository.DeleteSoftAsync(this);
                this.Committed = true;
            };
            this.MethodCalled = methodCalled;
            this.TypeName = typeof(T).FullName;
        }

        #region IDataStoreWriteEvent<T> Members

        public string TypeName { get; set; }
        public string MethodCalled { get; set; }
        public double QueryCost { get; set; }
        public TimeSpan QueryDuration { get; set; }
        public Func<Task> CommitClosure { get; set; }
        public bool Committed { get; set; }

        #endregion
    }
}