namespace DataStore.Interfaces
{
    using System.Linq;
    using DataStore.Interfaces.LowLevel;

    public interface IQueryOptions
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