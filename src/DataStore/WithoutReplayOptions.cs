namespace DataStore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using global::DataStore.Interfaces;
    using global::DataStore.Interfaces.LowLevel;
    using global::DataStore.Models.PureFunctions.Extensions;

    public class WithoutReplayOptions<T> : IWithoutReplayOptions, ISkipAndTake<T>, IOrderBy<T> where T : class, IAggregate, new()
    {
        private readonly Queue<(string, bool)> thenBys = new Queue<(string, bool)>();

        private bool orderByDirection;

        private string orderByProperty;

        private int skip;

        private int take;

        public WithoutReplayOptions<T> OrderBy(Expression<Func<T, object>> propertyRefExpr, bool descending = false)
        {
            this.orderByProperty = Objects.GetPropertyName(propertyRefExpr);
            this.orderByDirection = descending;
            return this;
        }

        public WithoutReplayOptions<T> Skip(int skip)
        {
            this.skip = skip;
            return this;
        }

        public WithoutReplayOptions<T> Take(int take)
        {
            this.take = take;
            return this;
        }

        public WithoutReplayOptions<T> ThenBy(Expression<Func<T, object>> propertyRefExpr, bool descending = false)
        {
            this.thenBys.Enqueue((Objects.GetPropertyName(propertyRefExpr), descending));
            return this;
        }

        IQueryable<T> IOrderBy<T>.AddOrderBy(IQueryable<T> queryable)
        {
            if (!string.IsNullOrEmpty(this.orderByProperty))
            {
                queryable = OrderBy(queryable, this.orderByProperty, this.orderByDirection);

                while (this.thenBys.Count > 0)
                {
                    var thenBy = this.thenBys.Dequeue();

                   queryable = ThenBy(queryable, thenBy.Item1, thenBy.Item2);
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
                        type,
                        property.PropertyType
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
                        type,
                        property.PropertyType
                    },
                    source.Expression,
                    Expression.Quote(orderByExpression));
                return (IOrderedQueryable<TEntity>)source.Provider.CreateQuery<TEntity>(resultExpression);
            }
        }

        IQueryable<T> ISkipAndTake<T>.AddSkip(IQueryable<T> queryable)
        {
            return this.skip > 0 ? queryable.Skip(this.skip) : queryable;
        }

        IQueryable<T> ISkipAndTake<T>.AddTake(IQueryable<T> queryable)
        {
            return this.take > 0 ? queryable.Take(this.take) : queryable;
        }
    }
}