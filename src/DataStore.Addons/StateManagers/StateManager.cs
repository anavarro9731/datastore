namespace DataStore.Infrastructure.Impl.StateManagers
{
    using System;
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

        public virtual void Dispose()
        {
            DocumentDbPrimary.Dispose();
        }

        public virtual void SubmitChanges()
        {
            
        }

        #endregion
    }
}