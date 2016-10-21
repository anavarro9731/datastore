namespace DataStore.DataAccess.Interfaces.Addons
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using DataAccess.Interfaces;

    public interface ISecureDataStore
    {
        IUserWithPermissions SecuredAgainst { get; }

        IDataStore Unsecured { get; set; }

        Task CommitChanges();

        Task<T> Create<T>(IApplicationPermission permission, T model, bool readOnly = false)
            where T : IAggregate, new();

        Task<IEnumerable<Guid>> DeleteHardWhere<T>(IApplicationPermission permission, Expression<Func<T, bool>> predicate)
            where T : IAggregate;

        Task<IEnumerable<T>> DeleteSoftWhere<T>(IApplicationPermission permission, Expression<Func<T, bool>> predicate)
            where T : IAggregate;

        void Dispose();

        Task<IEnumerable<T>> Read<T>(IApplicationPermission permission, Func<IQueryable<T>, IQueryable<T>> queryableExtension = null)
            where T : IAggregate;

        Task<IEnumerable<T>> ReadActive<T>(
            IApplicationPermission permission,
            Func<IQueryable<T>, IQueryable<T>> queryableExtension = null) where T : IAggregate;

        Task<T> ReadActiveById<T>(IApplicationPermission permission, Guid modelId) where T : IAggregate;

        Task<T> UpdateById<T>(IApplicationPermission permission, Guid id, Action<T> action) where T : IAggregate;

        Task<T> UpdateByIdUsingValuesFromAnotherInstance<T>(IApplicationPermission permission, Guid id, T src) where T : IAggregate;

        Task<T> UpdateUsingValuesFromAnotherInstanceWithTheSameId<T>(IApplicationPermission permission, T src) where T : IAggregate;

        Task<IEnumerable<T>> UpdateWhere<T>(
            IApplicationPermission permission,
            Expression<Func<T, bool>> predicate,
            Action<T> action,
            bool overwriteReadOnly = false) where T : IAggregate;

        ISecureDataStore UsingSecurityContext(IUserWithPermissions user);
    }
}