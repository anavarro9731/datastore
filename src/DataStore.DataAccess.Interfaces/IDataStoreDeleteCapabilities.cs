namespace DataAccess.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    public interface IDataStoreDeleteCapabilities
    {
        Task<T> DeleteHardById<T>(Guid id) where T : IAggregate;

        Task<IEnumerable<T>> DeleteHardWhere<T>(Expression<Func<T, bool>> predicate) where T : IAggregate;

        Task<T> DeleteSoftById<T>(Guid id) where T : IAggregate;

        Task<IEnumerable<T>> DeleteSoftWhere<T>(Expression<Func<T, bool>> predicate) where T : IAggregate;
    }
}