namespace DataStore.Infrastructure.Impl
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using DataAccess.Interfaces;
    using DataAccess.Interfaces.Addons;

    /// <summary>
    ///     a datastore capable of validating requests against a users permissions
    /// </summary>
    public class SecureDataStore : IDisposable, ISecureDataStore
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IDocumentRepository _repository;

        public SecureDataStore(IDocumentRepository repository, IEventAggregator eventAggregator, IUserWithPermissions securedAgainst)
        {
            _repository = repository;
            _eventAggregator = eventAggregator;
            Unsecured = new DataStore(repository, eventAggregator);
            SecuredAgainst = securedAgainst;
        }

        #region IDisposable Members

        public void Dispose()
        {
            Unsecured.Dispose();
        }

        #endregion

        #region ISecureDataStore Members

        public IUserWithPermissions SecuredAgainst { get; }

        public IDataStore Unsecured { get; set; }

        public void CommitChanges()
        {
            //this.Unsecured.CommitChanges();
        }

        public async Task<T> Create<T>(IApplicationPermission permission, T model, bool readOnly = false)
            where T : IAggregate, new()
        {
            AuthorizationHelper.Authorize(SecuredAgainst, permission, new IAggregate[] {model});

            return await Unsecured.Create(model, readOnly);
        }

        public Task<IEnumerable<Guid>> DeleteHardWhere<T>(IApplicationPermission permission, Expression<Func<T, bool>> predicate)
            where T : IAggregate
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<T>> DeleteSoftWhere<T>(IApplicationPermission permission, Expression<Func<T, bool>> predicate) where T : IAggregate
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<T>> Read<T>(IApplicationPermission permission, Func<IQueryable<T>, IQueryable<T>> queryableExtension = null)
            where T : IAggregate
        {
            var dataQueried = await Unsecured.Read(queryableExtension);

            AuthorizationHelper.Authorize(SecuredAgainst, permission, dataQueried.Cast<IAggregate>());

            return dataQueried;
        }

        public async Task<IEnumerable<T>> ReadActive<T>(
            IApplicationPermission permission,
            Func<IQueryable<T>, IQueryable<T>> queryableExtension = null) where T : IAggregate
        {
            var dataQueried = await Unsecured.ReadActive(queryableExtension);

            AuthorizationHelper.Authorize(SecuredAgainst, permission, dataQueried.Cast<IAggregate>());

            return dataQueried;
        }

        public async Task<T> ReadActiveById<T>(IApplicationPermission permission, Guid modelId) where T : IAggregate
        {
            var dataQueried = await Unsecured.ReadActiveById<T>(modelId);

            AuthorizationHelper.Authorize(SecuredAgainst, permission, new IAggregate[] {dataQueried});

            return dataQueried;
        }

        public Task<T> UpdateById<T>(IApplicationPermission permission, Guid id, Action<T> action) where T : IAggregate
        {
            throw new NotImplementedException();
        }

        public Task<T> UpdateByIdUsingValuesFromAnotherInstance<T>(IApplicationPermission permission, Guid id, T src) where T : IAggregate
        {
            throw new NotImplementedException();
        }

        public Task<T> UpdateUsingValuesFromAnotherInstanceWithTheSameId<T>(IApplicationPermission permission, T src) where T : IAggregate
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<T>> UpdateWhere<T>(
            IApplicationPermission permission,
            Expression<Func<T, bool>> predicate,
            Action<T> action,
            bool overwriteReadOnly = false) where T : IAggregate
        {
            throw new NotImplementedException();
        }

        public ISecureDataStore UsingSecurityContext(IUserWithPermissions user)
        {
            return new SecureDataStore(_repository, _eventAggregator, user);
        }

        #endregion
    }
}