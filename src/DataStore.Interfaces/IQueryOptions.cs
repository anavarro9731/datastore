namespace DataStore.Interfaces
{
    using System.Collections.Generic;
    using System.Linq;

    public interface IQueryOptions<T>
    {
    }

    public interface IWithoutReplayOptions<T> : IQueryOptions<T>, IContinueAndTake<T>, IOrderBy<T>
    {
    }

    public interface IContinueAndTake<T>
    {
        int? MaxTake { get; }
        ContinuationToken CurrentContinuationToken { get; }
        ContinuationToken NextContinuationToken { set; }
    }

    public interface IOrderBy<T>
    {
        IQueryable<T> AddOrderBy(IQueryable<T> queryable);

        List<(string, bool)> OrderByParameters { get; }
    }
}