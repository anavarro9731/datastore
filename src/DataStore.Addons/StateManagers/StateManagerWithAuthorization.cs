
namespace DataStore.Infrastructure.Impl.StateManagers
{
    using System;
    using System.Threading.Tasks;
    using DataAccess.Interfaces;
    using DataAccess.Interfaces.Addons;

    /// <summary>
    ///     a state manager which uses secure datastores
    /// </summary>
    public class StateManagerWithAuthorization : IStateManagerWithAuthorization
    {
        protected StateManagerWithAuthorization(IDocumentRepository repository, IEventAggregator eventAggregator, IUserWithPermissions user)
        {
            DocumentDbPrimary = new SecureDataStore(repository, eventAggregator, user);
            TransactionId = Guid.NewGuid();
        }

        #region ISecureStateManager Members

        public ISecureDataStore DocumentDbPrimary { get; }

        public Guid TransactionId { get; set; }

        public virtual async Task CommitChanges()
        {
            await DocumentDbPrimary.CommitChanges();
        }

        #endregion
    }
}