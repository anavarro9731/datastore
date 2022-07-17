namespace DataStore.Interfaces.Options.ClientSide.Interfaces
{
    using System;
    using System.Linq.Expressions;
    using DataStore.Interfaces.LowLevel;

    public interface IWithoutReplayOptionsClientSide<T> where T : class, IAggregate, new()
    {
        IWithoutReplayOptionsClientSide<T> ContinueFrom(ContinuationToken currentContinuationToken);

        IWithoutReplayOptionsClientSide<T> OrderBy(Expression<Func<T, object>> propertyRefExpr, bool descending = false);

        IWithoutReplayOptionsClientSide<T> Take(int take, ref ContinuationToken newContinuationToken);

        IWithoutReplayOptionsClientSide<T> ThenBy(Expression<Func<T, object>> propertyRefExpr, bool descending = false);
    }
}