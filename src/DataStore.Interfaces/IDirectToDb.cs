namespace DataStore.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using DataStore.Interfaces.LowLevel;

    public interface IDirectToDb
    {
        Task<int> Count<T>(Expression<Func<T, bool>> predicate = null) where T : class, IAggregate, new();

        Task<int> CountActive<T>(Expression<Func<T, bool>> predicate = null) where T : class, IAggregate, new();

        Task<IEnumerable<T>> Read<T>(Expression<Func<T, bool>> predicate = null, Func<IQueryOptions> queryOptions = null) where T : class, IAggregate, new();

        Task<T> ReadById<T>(Guid modelId) where T : class, IAggregate, new();
    }
}