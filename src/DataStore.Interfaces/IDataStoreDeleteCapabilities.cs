namespace DataStore.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using DataStore.Interfaces.LowLevel;

    public interface IDataStoreDeleteCapabilities
    {
        Task<T> DeleteHardById<T>(Guid id, string methodName = null) where T : class, IAggregate, new();

        Task<IEnumerable<T>> DeleteHardWhere<T>(Expression<Func<T, bool>> predicate, string methodName = null) where T : class, IAggregate, new();

        Task<T> DeleteSoftById<T>(Guid id, string methodName = null) where T : class, IAggregate, new();

        Task<IEnumerable<T>> DeleteSoftWhere<T>(Expression<Func<T, bool>> predicate, string methodName = null) where T : class, IAggregate, new();
    }
}