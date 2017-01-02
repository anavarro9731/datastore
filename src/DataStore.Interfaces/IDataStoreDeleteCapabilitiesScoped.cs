namespace DataStore.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    public interface IDataStoreDeleteCapabilitiesScoped<T> where T : IAggregate
    {
        Task<T> DeleteHardById(Guid id);

        Task<IEnumerable<T>> DeleteHardWhere(Expression<Func<T, bool>> predicate);

        Task<T> DeleteSoftById(Guid id);

        Task<IEnumerable<T>> DeleteSoftWhere(Expression<Func<T, bool>> predicate);
    }
}