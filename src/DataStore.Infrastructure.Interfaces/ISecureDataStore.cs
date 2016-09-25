namespace Infrastructure.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using DataAccess.Interfaces;
    using DataAccess.Models;

    public interface ISecureDataStore
    {
        IUserWithPermissions SecuredAgainst { get; }

        IDataStore Unsecured { get; set; }

        void CommitChanges();

        Task<T> Create<T>(IApplicationPermission permission, T model, bool readOnly = false, bool hidden = false)
            where T : Aggregate, new();

        Task<IEnumerable<Guid>> DeleteHardWhere<T>(IApplicationPermission permission, Expression<Func<T, bool>> predicate)
            where T : Aggregate;

        Task<IEnumerable<T>> DeleteSoftWhere<T>(IApplicationPermission permission, Expression<Func<T, bool>> predicate)
            where T : Aggregate;

        void Dispose();

        Task<IEnumerable<T>> Read<T>(IApplicationPermission permission, Func<IQueryable<T>, IQueryable<T>> queryableExtension = null)
            where T : Aggregate;

        Task<IEnumerable<T>> ReadActive<T>(
            IApplicationPermission permission,
            Func<IQueryable<T>, IQueryable<T>> queryableExtension = null,
            bool includeHidden = false) where T : Aggregate;

        Task<T> ReadActiveById<T>(IApplicationPermission permission, Guid modelId) where T : Aggregate;

        Task<T> UpdateById<T>(IApplicationPermission permission, Guid id, Action<T> action) where T : Aggregate;

        Task<T> UpdateByIdUsingValuesFromAnotherInstance<T>(IApplicationPermission permission, Guid id, T src) where T : Aggregate;

        Task<T> UpdateUsingValuesFromAnotherInstanceWithTheSameId<T>(IApplicationPermission permission, T src) where T : Aggregate;

        Task<IEnumerable<T>> UpdateWhere<T>(
            IApplicationPermission permission,
            Expression<Func<T, bool>> predicate,
            Action<T> action,
            bool overwriteReadOnly = false) where T : Aggregate;

        ISecureDataStore UsingSecurityContext(IUserWithPermissions user);
    }
}