namespace DataStore.Infrastructure.Impl.StateManagers
{
    using System;
    using System.Threading.Tasks;
    using DataAccess.Interfaces;
    using DataAccess.Interfaces.Addons;

    public class StateManager : IStateManagerWithoutAuthorization
    {
        public StateManager(IDocumentRepository repository, IEventAggregator eventAggregator)
        {
            DocumentDbPrimary = new DataStore(repository, eventAggregator);
            TransactionId = Guid.NewGuid();
        }

        #region IStateManagerWithoutAuthorization Members

        public IDataStore DocumentDbPrimary { get; }

        public Guid TransactionId { get; set; }
        
        public virtual async Task CommitChanges()
        {
            await DocumentDbPrimary.CommitChanges();
        }

        #endregion
    }
}