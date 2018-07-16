namespace DataStore.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using DataStore.Interfaces.LowLevel;

    public interface IDataStoreUpdateCapabilities
    {
        Task<T> Update<T>(T src, bool overwriteReadOnly = false, string methodName = null) where T : class, IAggregate, new();

        Task<T> UpdateById<T>(Guid id, Action<T> action, bool overwriteReadOnly = false, string methodName = null) where T : class, IAggregate, new();

        Task<IEnumerable<T>> UpdateWhere<T>(Expression<Func<T, bool>> predicate, Action<T> action, bool overwriteReadOnly = false, string methodName = null)
            where T : class, IAggregate, new();
    }
}