namespace DataStore.Infrastructure.Impl.StateManagers
{
    using System;
    using DataAccess.Interfaces;
    using DataAccess.Interfaces.Addons;

    /// <summary>
    ///     a state manager which uses secure datastores
    /// </summary>
    public class SecureStateManager : ISecureStateManager
    {
        protected SecureStateManager(IDocumentRepository repository, IEventAggregator eventAggregator, IUserWithPermissions user)
        {
            GlobalStore = new SecureDataStore(repository, eventAggregator, user);
            TransactionId = Guid.NewGuid();
        }

        #region ISecureStateManager Members

        public ISecureDataStore GlobalStore { get; }

        public Guid TransactionId { get; set; }

        public virtual void Dispose()
        {
            GlobalStore.Dispose();
        }

        public virtual void SubmitChanges()
        {
            GlobalStore.CommitChanges();
        }

        #endregion
    }
}