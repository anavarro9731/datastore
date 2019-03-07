namespace DataStore.Interfaces
{
    using System.Linq;

    public interface IQueryOptions<T>
    {
    }

    public interface IWithoutReplayOptions<T> : IQueryOptions<T>, ISkipAndTake<T>, IOrderBy<T>
    {
    }

    public interface ISkipAndTake<T>
    {
        IQueryable<T> AddSkip(IQueryable<T> queryable);

        IQueryable<T> AddTake(IQueryable<T> queryable);
    }

    public interface IOrderBy<T>
    {
        IQueryable<T> AddOrderBy(IQueryable<T> queryable);
    }
}