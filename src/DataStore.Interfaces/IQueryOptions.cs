namespace DataStore.Interfaces
{
    using System.Collections.Generic;
    using System.Linq;

    public interface IQueryOptions<T>
    {
    }

    public interface IWithoutReplayOptions<T> : IQueryOptions<T>, ISkipAndTake<T>, IOrderBy<T>
    {
    }

    public interface ISkipAndTake<T>
    {
        Queue<IQueryable<T>> AddSkipAndTake(IQueryable<T> queryable, int? maxTake);
        Queue<IQueryable<T>> AddSkipAndTake(IQueryable<T> queryable);
    }

    public interface IOrderBy<T>
    {
        IQueryable<T> AddOrderBy(IQueryable<T> queryable);
    }
}