namespace DataStore.Interfaces.Options
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using DataStore.Interfaces.LowLevel;
    using DataStore.Interfaces.LowLevel.Permissions;

    public abstract class WithoutReplayOptionsClientSide<T> where T : class, IAggregate, new()
    {
        public static implicit operator WithoutReplayOptionsLibrarySide<T>(WithoutReplayOptionsClientSide<T> options) =>
            options.LibrarySide;

        protected WithoutReplayOptionsClientSide(WithoutReplayOptionsLibrarySide<T> librarySide)
        {
            LibrarySide = librarySide;
        }

        protected WithoutReplayOptionsLibrarySide<T> LibrarySide { get; }

        //* visible members

        public abstract void AuthoriseFor(IIdentityWithDatabasePermissions identity);

        public abstract WithoutReplayOptionsClientSide<T> ContinueFrom(ContinuationToken currentContinuationToken);

        public abstract WithoutReplayOptionsClientSide<T> OrderBy(Expression<Func<T, object>> propertyRefExpr, bool descending = false);

        public abstract WithoutReplayOptionsClientSide<T> Take(int take, ref ContinuationToken newContinuationToken);

        public abstract WithoutReplayOptionsClientSide<T> ThenBy(Expression<Func<T, object>> propertyRefExpr, bool descending = false);
    }

    public class WithoutReplayOptionsLibrarySide<T>
    {
        public readonly Queue<(string, bool)> ThenByQueue = new Queue<(string, bool)>();

        public ContinuationToken CurrentContinuationToken;

        public IIdentityWithDatabasePermissions Identity;

        public int? MaxTake;

        public ContinuationToken NextContinuationToken;

        public string OrderByProperty;

        public bool OrderDescending;

        public ContinuationToken NextContinuationTokenValue { set => this.NextContinuationToken.Value = value.Value; }

        public List<(string, bool)> OrderByParameters { get; } = new List<(string, bool)>();

        public IQueryable<T> AddOrderBy(IQueryable<T> queryable)
        {
            if (!string.IsNullOrEmpty(this.OrderByProperty))
            {
                queryable = OrderBy(queryable, this.OrderByProperty, this.OrderDescending);

                OrderByParameters.Add((this.OrderByProperty, this.OrderDescending));

                while (this.ThenByQueue.Count > 0)
                {
                    var thenBy = this.ThenByQueue.Dequeue();

                    queryable = ThenBy(queryable, thenBy.Item1, thenBy.Item2);

                    OrderByParameters.Add(thenBy);
                }
            }

            return queryable;

            IOrderedQueryable<TEntity> OrderBy<TEntity>(IQueryable<TEntity> source, string orderByProperty, bool desc)
            {
                var command = desc ? "OrderByDescending" : "OrderBy";
                var type = typeof(TEntity);
                var property = type.GetProperty(orderByProperty);
                var parameter = Expression.Parameter(type, "p");
                var propertyAccess = Expression.MakeMemberAccess(parameter, property);
                var orderByExpression = Expression.Lambda(propertyAccess, parameter);
                var resultExpression = Expression.Call(
                    typeof(Queryable),
                    command,
                    new[]
                    {
                        type, property.PropertyType
                    },
                    source.Expression,
                    Expression.Quote(orderByExpression));
                return (IOrderedQueryable<TEntity>)source.Provider.CreateQuery<TEntity>(resultExpression);
            }

            IOrderedQueryable<TEntity> ThenBy<TEntity>(IQueryable<TEntity> source, string orderByProperty, bool desc)
            {
                var command = desc ? "ThenByDescending" : "ThenBy";
                var type = typeof(TEntity);
                var property = type.GetProperty(orderByProperty);
                var parameter = Expression.Parameter(type, "p");
                var propertyAccess = Expression.MakeMemberAccess(parameter, property);
                var orderByExpression = Expression.Lambda(propertyAccess, parameter);
                var resultExpression = Expression.Call(
                    typeof(Queryable),
                    command,
                    new[]
                    {
                        type, property.PropertyType
                    },
                    source.Expression,
                    Expression.Quote(orderByExpression));
                return (IOrderedQueryable<TEntity>)source.Provider.CreateQuery<TEntity>(resultExpression);
            }
        }
    }
}