namespace DataStore.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using LowLevel;

    public interface IDataStoreUpdateCapabilities
    {
        Task<T> UpdateById<T>(Guid id, Action<T> action, bool overwriteReadOnly = false) where T : class, IAggregate, new();

        Task<T> Update<T>(T src, bool overwriteReadOnly = false) where T : class, IAggregate, new();

        Task<IEnumerable<T>> UpdateWhere<T>(
            Expression<Func<T, bool>> predicate,
            Action<T> action,
            bool overwriteReadOnly = false) where T : class, IAggregate, new();
    }
}