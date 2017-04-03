namespace DataStore.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using LowLevel;

    public interface IDataStoreUpdateCapabilitiesScoped<T> where T : IAggregate
    {
        Task<T> UpdateById(Guid id, Action<T> action, bool overwriteReadOnly = false);

        Task<T> Update(T src, bool overwriteReadOnly = false);

        Task<IEnumerable<T>> UpdateWhere(
            Expression<Func<T, bool>> predicate,
            Action<T> action,
            bool overwriteReadOnly = false);
    }
}