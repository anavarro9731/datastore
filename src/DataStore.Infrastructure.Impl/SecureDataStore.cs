namespace Infrastructure.HandlerServices
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    using DataAccess.Interfaces;
    using DataAccess.Models;

    using DataStore;

    using Infrastructure.HandlerServiceInterfaces;
    using Infrastructure.Objects.Permissions;

    /// <summary>
    ///     a datastore capable of validating requests against a users permissions
    /// </summary>
    public class SecureDataStore : IDisposable, ISecureDataStore
    {
        private readonly IDocumentRepository repository;

        private readonly IEventAggregator eventAggregator;

        public SecureDataStore(IDocumentRepository repository, IEventAggregator eventAggregator, IUserWithPermissions securedAgainst)
        {
            this.repository = repository;
            this.eventAggregator = eventAggregator;
            Unsecured = new DataStore(repository, eventAggregator);
            SecuredAgainst = securedAgainst;
        }

        public IUserWithPermissions SecuredAgainst { get; }

        public IDataStore Unsecured { get; set; }

        public void CommitChanges()
        {
            //this.Unsecured.CommitChanges();
        }

        public async Task<T> Create<T>(IApplicationPermission permission, T model, bool readOnly = false, bool hidden = false)
            where T : Aggregate, new()
        {
            AuthorizationHelper.Authorize(this.SecuredAgainst, permission, new[] { model });

            return await this.Unsecured.Create(model, readOnly, hidden);
        }

        public Task<IEnumerable<Guid>> DeleteHardWhere<T>(IApplicationPermission permission, Expression<Func<T, bool>> predicate)
            where T : Aggregate
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<T>> DeleteSoftWhere<T>(IApplicationPermission permission, Expression<Func<T, bool>> predicate) where T : Aggregate
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            this.Unsecured.Dispose();
        }

        public async Task<IEnumerable<T>> Read<T>(IApplicationPermission permission, Func<IQueryable<T>, IQueryable<T>> queryableExtension = null)
            where T : Aggregate
        {
            var dataQueried = await this.Unsecured.Read(queryableExtension);

            AuthorizationHelper.Authorize(this.SecuredAgainst, permission, dataQueried);

            return dataQueried;
        }

        public async Task<IEnumerable<T>> ReadActive<T>(
            IApplicationPermission permission,
            Func<IQueryable<T>, IQueryable<T>> queryableExtension = null,
            bool includeHidden = false) where T : Aggregate
        {
            var dataQueried = await this.Unsecured.ReadActive(queryableExtension, includeHidden);

            AuthorizationHelper.Authorize(this.SecuredAgainst, permission, dataQueried);

            return dataQueried;
        }

        public async Task<T> ReadActiveById<T>(IApplicationPermission permission, Guid modelId) where T : Aggregate
        {
            var dataQueried = await this.Unsecured.ReadActiveById<T>(modelId);

            AuthorizationHelper.Authorize(this.SecuredAgainst, permission, new[] { dataQueried });

            return dataQueried;
        }

        public Task<T> UpdateById<T>(IApplicationPermission permission, Guid id, Action<T> action) where T : Aggregate
        {
            throw new NotImplementedException();
        }

        public Task<T> UpdateByIdUsingValuesFromAnotherInstance<T>(IApplicationPermission permission, Guid id, T src) where T : Aggregate
        {
            throw new NotImplementedException();
        }

        public Task<T> UpdateUsingValuesFromAnotherInstanceWithTheSameId<T>(IApplicationPermission permission, T src) where T : Aggregate
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<T>> UpdateWhere<T>(
            IApplicationPermission permission,
            Expression<Func<T, bool>> predicate,
            Action<T> action,
            bool overwriteReadOnly = false) where T : Aggregate
        {
            throw new NotImplementedException();
        }

        public ISecureDataStore UsingSecurityContext(IUserWithPermissions user)
        {
            return new SecureDataStore(repository, eventAggregator, user);
        }
    }
}