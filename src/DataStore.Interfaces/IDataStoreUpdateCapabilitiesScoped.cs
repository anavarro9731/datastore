namespace DataStore.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using DataStore.Interfaces.LowLevel;

    public interface IDataStoreUpdateCapabilitiesScoped<T> where T : class, IAggregate, new()
    {
        Task<T> Update(T src, bool overwriteReadOnly = false);

        Task<T> UpdateById(Guid id, Action<T> action, bool overwriteReadOnly = false);

        Task<IEnumerable<T>> UpdateWhere(Expression<Func<T, bool>> predicate, Action<T> action, bool overwriteReadOnly = false);
    }
}