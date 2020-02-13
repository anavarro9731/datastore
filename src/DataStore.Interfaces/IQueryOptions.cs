namespace DataStore.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using DataStore.Interfaces.LowLevel;

    public interface IQueryOptions<T>
    {
    }

    public interface IWithoutReplayOptionsClientSide<T> where T : class, IAggregate, new()
    {
        IWithoutReplayOptionsClientSide<T> ContinueFrom(ContinuationToken currentContinuationToken);

        IWithoutReplayOptionsClientSide<T> OrderBy(Expression<Func<T, object>> propertyRefExpr, bool descending = false);

        IWithoutReplayOptionsClientSide<T> Take(int take, ref ContinuationToken newContinuationToken);

        IWithoutReplayOptionsClientSide<T> ThenBy(Expression<Func<T, object>> propertyRefExpr, bool descending = false);
    }

    public interface IWithoutReplayOptionsLibrarySide<T> : IQueryOptions<T>, IContinueAndTake<T>, IOrderBy<T>
        where T : class, IAggregate, new()
    {
    }

    //-visible to repo side
    public interface IContinueAndTake<T>
    {
        ContinuationToken CurrentContinuationToken { get; }

        int? MaxTake { get; }

        ContinuationToken NextContinuationToken { set; }
    }

    //-visible to repo side
    public interface IOrderBy<T>
    {
        List<(string, bool)> OrderByParameters { get; }

        IQueryable<T> AddOrderBy(IQueryable<T> queryable);
    }
}